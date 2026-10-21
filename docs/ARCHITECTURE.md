# Architecture

## 1. Shape of the system

```text
┌──────────────────────────────────────────────────────────────────────┐
│  FluxConvert.App  (WPF, net8.0-windows)                              │
│                                                                      │
│   Views            ViewModels            Controls / Converters       │
│   ┌──────────┐     ┌───────────────┐      ┌────────────────────┐    │
│   │ Radial   │◄───►│ RadialMenuVM  │      │ RadialItemControl  │    │
│   │ Window   │     │ ShellVM       │      │ RingSurface        │    │
│   │ Queue    │     │ SettingsVM    │      │ ProgressRing       │    │
│   │ Window   │     │ HistoryVM     │      └────────────────────┘    │
│   └──────────┘     └───────────────┘                                │
│         │                  │                                         │
│         │            ┌─────▼─────────────────────────────────┐      │
│         │            │  Application services (DI singletons) │      │
│         │            │  FileDetectionService                │      │
│         │            │  MediaMetadataService                │      │
│         │            │  FormatCapabilityService             │      │
│         │            │  ConversionService                   │      │
│         │            │  ConversionQueueService              │      │
│         │            │  SettingsService / HistoryService    │      │
│         │            └─────┬─────────────────────────────────┘      │
│         │                  │                                         │
│         │        ┌─────────▼──────────┐  ┌──────────────┐           │
│         │        │ ProcessRunner      │  │ ToolLocator  │           │
│         │        │ (async, cancellable)│  │ (ffmpeg/magick)│         │
│         │        └─────────┬──────────┘  └──────┬───────┘           │
└────────┼───────────────────┼──────────────────┼─────────────────────┘
         │                   │                  │
         │            ┌──────▼──────────────────▼───────┐
         │            │  FluxConvert.Core (net8.0)      │
         │            │  domain models, capability       │
         │            │  registry, settings, history,    │
         │            │  filename + collision policy     │
         │            └──────────────────────────────────┘
         │
   external tools: ffmpeg.exe, ffprobe.exe, magick.exe
```

`FluxConvert.Core` holds everything that can be unit-tested without a UI: file classification,
format capability rules, output filename generation and collision policy, settings serialisation,
queue state transitions, and error mapping. It has **no WPF dependency**, which is what makes the
test project fast and meaningful.

`FluxConvert.App` is the WPF shell: views, view models, the radial menu, the tray icon, and the
adapters that bind core services to the UI.

`FluxConvert.Core.Tests` covers the core library with xUnit.

## 2. Dependency policy

| Package | Purpose | Why |
| --- | --- | --- |
| `CommunityToolkit.Mvvm` | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`, source generators | Maintained by Microsoft; no runtime baggage beyond the generator output |
| `Microsoft.Extensions.DependencyInjection` | Composition root | Requested by the brief; used where it genuinely helps |
| `Microsoft.Extensions.Logging` | `ILogger<T>` abstraction | Requested by the brief |
| `Microsoft.Extensions.Logging.Abstractions` | Shared by the above | — |
| `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk` | Testing | Standard |

Deliberately **not** used: Serilog (a small file logger is written in-house), any UI framework
beyond plain WPF, any icon package (an original icon set is authored as vector `Geometry`
resources), and any network/HTTP client.

## 3. Key design decisions

### 3.1 `net8.0-windows` instead of `net10.0-windows`

The environment only has the .NET 8.0.100 SDK. See [`PROGRESS.md`](PROGRESS.md) for the full
finding. The target framework lives in the `.csproj` files and one `Directory.Build.props`
property, so the upgrade is trivial.

### 3.2 The radial menu is a real interactive control

The ring is drawn by a custom `FrameworkElement` (`RingSurface`) that renders the glass annulus,
the item badges, and the progress arc with `OnRender`. The items themselves are **real WPF
`Button` controls** positioned by the ring at runtime and hosted in an overlay `Canvas`. This
gives free keyboard navigation, focus rings, screen-reader names, and automation IDs, while the
custom drawing supplies the visual quality that a plain `ItemsControl` cannot.

The alternative — a single custom-drawn canvas with manual hit-testing — was rejected because it
would have to re-implement focus, keyboard navigation, and accessibility from scratch.

### 3.3 The floating window is a borderless, transparent, topmost `Window`

`AllowsTransparency="True"`, `WindowStyle="None"`, `Background="Transparent"`. The window is
resized to exactly the widget's needs and repositioned inside the virtual screen bounds so it
never falls off-screen on a multi-monitor setup. Clicking outside does **not** steal focus from
other applications (no `Activate()` unless the user explicitly summoned the widget), so the
floating target never interrupts work.

### 3.4 External tools are located, never assumed

`IToolLocator` resolves `ffmpeg.exe`, `ffprobe.exe` and `magick.exe` in this order:

1. An explicit path from settings (`Advanced` section).
2. A bundled copy under `tools/` (development) or `Tools/` next to the executable (release).
3. The system `PATH`.

Nothing is hard-coded to a specific absolute path. When a tool is missing, the capability service
reports it as unavailable and the UI *disables the affected options with an explanation* rather
than failing at conversion time.

### 3.5 Process execution is centralised

All external processes go through `ProcessRunner`, which:

- takes a file name and an **argument list** (never a concatenated command string), so paths with
  spaces, quotes, `&`, `|`, or non-ASCII characters are passed as single arguments and are never
  interpreted by a shell;
- reads stdout and stderr concurrently and asynchronously to avoid pipe deadlocks;
- enforces a hard timeout and supports cooperative cancellation (kill the process tree on cancel);
- returns exit code, captured output and duration;
- never runs on the UI thread.

FFmpeg's progress output (`-progress pipe:1`) and ImageMagick's stderr output are parsed by
dedicated parsers (`FfmpegProgressParser`, `ImageMagickProgressParser`) that are pure functions
over strings — fully unit-testable without running a process.

### 3.6 Conversions are atomic

Every conversion writes to a temporary file in the destination directory (`.fluxconvert-tmp-*`),
and the file is moved into its final name only after the tool exits successfully **and** the
result passes validation. A crashed or cancelled conversion never leaves a half-written file
under the user's chosen name, and the original file is never modified.

### 3.7 Collision policy is deterministic and testable

`OutputPathBuilder` produces `<name>.<ext>` and, when that path already exists, appends
`(2)`, `(3)`, … before the extension. It never overwrites. The policy lives in Core and is
covered by unit tests.

### 3.8 The notification-area icon uses one Windows Forms control

`System.Windows.Forms.NotifyIcon` is the only Windows Forms type in the product; the user
interface stays in WPF. Implementing `Shell_NotifyIcon` by hand would mean re-implementing
notification-area registration, version negotiation and icon lifetime management for one icon.

Because enabling Windows Forms adds `System.Windows.Forms` and `System.Drawing` as *implicit*
usings, both are removed again with `<Using Remove="..." />` in the project file. The files that
genuinely need them import them explicitly, so `Application`, `Color` and `Pen` never become
ambiguous with their WPF counterparts.

The Windows Forms SDK also warns (`WFAC010`) that DPI awareness should be declared through
`ApplicationHighDpiMode` rather than the application manifest. That property only configures the
scaling of Windows Forms *forms*, which this product does not have, and it was measured to leave
the process `SYSTEM_DPI_AWARE`. The manifest therefore declares Per-Monitor-V2 by hand, and the
warning is suppressed with the reason recorded in `app.manifest` and in the project file.

### 3.9 Only one copy of the application runs

`SingleInstanceCoordinator` claims a named mutex (`Local\<name>.SingleInstance`) and listens on a
named auto-reset event (`Local\<name>.Activate`). A second launch finds the mutex taken, signals
the event, and exits; the running instance brings its interface back to the front.

A named mutex is *re-entrant* for the thread that already owns it, so the mutex alone cannot
distinguish "a second process" from "a second coordinator in the same process". A process-wide
claim backs the mutex up so the two cases cannot be confused.

### 3.10 Logging is structured and privacy-conscious

`Microsoft.Extensions.Logging` with a custom file provider writing daily rolling files under
`%LOCALAPPDATA%\FluxConvert\logs`. File paths of converted media are logged (needed for support),
but the contents of files are never inspected beyond format detection, and no file content or
user-identifying data is transmitted anywhere.

### 3.11 Settings and history are JSON

`System.Text.Json` with explicit `JsonSerializerOptions` (camelCase, indented, tolerant of
unknown members so a downgrade does not crash). Writes go to a temporary file that is then
atomically replaced, so a crash mid-write cannot corrupt the settings file.

Application data root: `%LOCALAPPDATA%\FluxConvert\`
(`settings.json`, `history.json`, `logs\`). The path is resolved from
`Environment.SpecialFolder.LocalApplicationData`.

## 4. Third-party tooling

| Tool | Role | License status | How it is obtained |
| --- | --- | --- | --- |
| FFmpeg / FFprobe | Video + audio conversion, metadata | **Unverified** — GPL/LGPL depending on the build | Not bundled yet; located at runtime, see F-401/F-902 |
| ImageMagick | Image conversion | **Unverified** — Apache-2.0 (ImageMagick Studio is a different distribution) | Not bundled yet; located at runtime, see F-401/F-902 |

**Rule.** No third-party executable is committed to this repository or added to a release build
until its license has been verified and recorded in `THIRD-PARTY-NOTICES`. Until then the
`tools/` directory is git-ignored and used only for local development.

## 5. Architecture Decision Records

Short ADRs live in [`adr/`](adr/). Each records context, decision, consequences.

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0001](adr/0001-target-framework.md) | Target `net8.0-windows` | Accepted |
| [ADR-0002](adr/0002-external-tool-discovery.md) | Locate media tools at runtime | Accepted |
| [ADR-0003](adr/0003-radial-menu-interaction-model.md) | Real controls over custom hit-testing | Accepted |
