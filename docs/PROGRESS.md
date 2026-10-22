# Progress Log

Status legend: **Planned** · **In Progress** · **Implemented** · **Verified** · **Blocked**

`Verified` means the code was compiled, the relevant tests were executed, and the result was
observed. It never means "the code looks right".

---

## 2026-10-21 — Phase 0: assessment and baseline

### F-000 · Repository, Git and toolchain assessment — **Verified**

**Inspected**

- Working tree: `C:\Users\OMISK\Desktop\DVIMC\DVIMC`
- Branch: `main`, tracking `origin/main`, clean working tree at commit `1738ae4` ("Initial commit").
- Repository contents: only `.gitattributes`. No existing application code, no documentation, no
  agent instructions, no uncommitted user work to preserve.
- Remote: `https://github.com/Omiskn/DVIMC.git` — left untouched.

**Toolchain**

| Tool | Finding |
| --- | --- |
| .NET SDK | **8.0.100** only. No .NET 9/10 runtime installed. |
| .NET runtimes | 8.0.0 / 8.0.23, incl. `Microsoft.WindowsDesktop.App` (required for WPF). |
| FFmpeg / FFprobe | **Not installed** (`Get-Command ffmpeg` → not found). |
| ImageMagick | **Not installed** (`Get-Command magick` → not found). |
| `winget` | Present at `C:\Users\OMISK\AppData\Local\Microsoft\WindowsApps\winget.exe`. |
| Network | nuget.org reachable; GitHub reachable. |
| Display | One display, 1920×1080 physical, 1536×864 logical, 125 % scaling (`AppliedDPI` = 120). |

**Deviation from the brief.** The brief asks for .NET 10 LTS. Only the .NET 8 SDK is installed in
this environment, and no .NET 10 SDK could be installed without altering the user's machine
toolchain. The project therefore targets **`net8.0-windows`**, which is the newest LTS actually
available here. The target framework is a single property; moving to `net10.0-windows` later is a
one-line change plus a newer SDK, with no source impact.

**Media tooling decision.** FFmpeg and ImageMagick are *not* installed. Rather than modifying the
user's system, the application will resolve both tools through a locator service
(settings override → bundled `tools/` directory → system `PATH`). Integration tests skip
gracefully when the tools are absent, so the build stays verifiable on a clean machine.

**Visual reference.** `https://radial.appverge.net` was fetched and inspected. It is a macOS
product landing page for a paid app; the radial widget itself is only visible in the hero banner
image. Findings and assumptions are recorded in [`ART_DIRECTION.md`](ART_DIRECTION.md).

### F-001 · Baseline documentation and repository scaffolding — **Verified**

**Created**

- `README.md` — overview, prerequisites, dev setup, layout, privacy.
- `PROJECT_STATUS.md` — canonical project status (mandatory record).
- `docs/ROADMAP.md` — phased feature plan with IDs, dependencies and status.
- `docs/PROGRESS.md` — this log.
- `docs/ARCHITECTURE.md` — component map, design decisions, ADR index.
- `docs/TESTING.md` — test strategy and manual validation procedures.
- `docs/ART_DIRECTION.md` — reference analysis and design language.
- `.gitignore` — build output, IDE files, `tools/` (external binaries), local overrides.
- `.editorconfig` — C# / XAML / JSON formatting and naming conventions.
- `Directory.Build.props` — shared compiler, nullable, analyzer and metadata settings.
- `.gitattributes` — extended with binary rules for media and binary assets.

**Not yet done in this phase:** no application code exists yet. That is F-101 and later.

### F-101 · Solution and project skeleton — **Verified**

**Created**

```text
FluxConvert.sln
src/FluxConvert.Core/          net8.0 class library, no WPF dependency
  FluxConvert.Core.csproj      + Microsoft.Extensions.Logging.Abstractions 8.0.3
  ProductInfo.cs               single source of product identity
src/FluxConvert.App/           net8.0-windows WPF application
  FluxConvert.App.csproj       + CommunityToolkit.Mvvm 8.4.2, M.E.DI 8.0.1, M.E.Logging 8.0.1
  App.xaml(.cs)                entry point; window created explicitly (no StartupUri)
  ShellWindow.xaml(.cs)        temporary shell, replaced by the radial widget in Phase 2
  Themes/Palette.xaml          colour tokens from docs/ART_DIRECTION.md § 3.1
  app.manifest                 PerMonitorV2 DPI awareness + long-path awareness
tests/FluxConvert.Core.Tests/  net8.0 xUnit project
  ProductInfoTests.cs          4 tests
  .editorconfig                relaxes CA1707 for the `Subject_Behaviour` test naming convention
```

**Build:** `dotnet build FluxConvert.sln -c Debug` → *Build succeeded, 0 Warning(s), 0 Error(s)*.

**Tests:** `dotnet test FluxConvert.sln -c Debug` → *Failed: 0, Passed: 4, Skipped: 0*.

**Manual verification.** `FluxConvert.App.exe` was launched from the build output. The process
stayed alive, exposed a main window titled `FluxConvert`, rendered the dark shell correctly
(screenshot captured), and terminated cleanly when stopped.

**Issue found and fixed during this feature.** The WPF markup compiler only generates
`Application.InitializeComponent()` when `App.xaml` contains loadable content — with an empty
resource dictionary and no `StartupUri` the build failed with `CS0103: The name 'InitializeComponent'
does not exist`. Fixed by merging `Themes/Palette.xaml` into `App.xaml`; the palette file must
therefore never be emptied. The behaviour was reproduced in a scratch project before being
diagnosed.

**Test issue found and fixed.** `AssemblyMetadataAttribute` was asserted for the product name, but
the SDK emits `AssemblyProductAttribute` for `$(Product)`. The test was corrected accordingly.

---

### F-102 · Composition root, file logging and settings persistence — **Verified**

**Created**

```text
src/FluxConvert.Core/Infrastructure/AppPaths.cs          application data root resolution
src/FluxConvert.Core/Infrastructure/FileLoggerProvider.cs daily rotating file logger
src/FluxConvert.Core/Settings/AppSettings.cs              persisted preferences
src/FluxConvert.Core/Settings/ISettingsService.cs         settings contract
src/FluxConvert.Core/Settings/SettingsService.cs          atomic JSON load/save
src/FluxConvert.App/Composition/AppServices.cs             DI composition root
src/FluxConvert.App/App.xaml.cs                            builds and disposes the container
tests/FluxConvert.Core.Tests/AppPathsTests.cs              5 tests
tests/FluxConvert.Core.Tests/FileLoggerProviderTests.cs    5 tests
tests/FluxConvert.Core.Tests/SettingsServiceTests.cs      9 tests + 5 theory cases
```

**Behaviour**

- `AppPaths` resolves the data root from `FLUXCONVERT_DATA_ROOT`, then
  `%LOCALAPPDATA%\FluxConvert`, then `%APPDATA%\FluxConvert`, then `<exe>\data`. `EnsureCreated()`
  creates `logs\` and `temp\`.
- `FileLoggerProvider` writes `fluxconvert-YYYYMMDD.log` with a
  `timestamp [LVL] Category: message` line format. The file is opened for the duration of a
  single write and closed again, so the log is never locked and can be opened in Notepad while
  the application runs.
- `SettingsService` reads and writes `settings.json` (camelCase, indented). Writes are atomic
  (temp file + move). A corrupt file is moved aside as `settings.corrupt-<timestamp>.json` and
  the defaults are used, so a bad file can never prevent startup. Unknown JSON members are
  ignored so a newer file does not break an older build.
- `AppServices` is the composition root. The container is validated at build time
  (`ValidateOnBuild`), so a missing or mis-registered dependency fails immediately instead of at
  first use. It is disposed on shutdown, which releases the log write lock.

**Build:** `dotnet build FluxConvert.sln -c Debug` → *Build succeeded, 0 Warning(s), 0 Error(s)*.

**Tests:** `dotnet test FluxConvert.sln -c Debug` → *Failed: 0, Passed: 27, Skipped: 0*.

**Manual verification.** The application was launched from the build output with an empty data
root. It created `%LOCALAPPDATA%\FluxConvert\{logs,temp}`, wrote
`fluxconvert-20261021.log` containing
`2026-10-21T08:36:34.568-07:00 [INF] FluxConvert.Core.Settings.SettingsService/LogNoSettingsFile: No settings file found at ...; using defaults.`,
showed its window and shut down cleanly.

**Issues found and fixed during this feature**

- The WPF SDK uses a reduced implicit-usings set that does **not** include `System.IO`; the App
  project needs an explicit `using System.IO;`.
- `CA1848` requires source-generated logging, so `SettingsService` now uses `[LoggerMessage]`
  partial methods.
- `CA1001` flagged the `SemaphoreSlim` in `SettingsService`; the service now implements
  `IDisposable` and is disposed by the container.
- `Microsoft.Extensions.Logging.Abstractions.NullScope` is internal; the logger provider now
  carries its own no-op scope.
- Holding the log file open for the lifetime of the provider made it unreadable by other
  processes (a `File.ReadAllText` failed with a sharing violation). The provider now opens and
  closes the file per write.

---

### F-103 · External media tool locator and process runner — **Verified**

Delivered ahead of the Phase 4 position it was planned for, because the version probe needs a
process runner and both are prerequisites for every conversion engine.

**Created**

```text
src/FluxConvert.Core/Process/ProcessRequest.cs         executable + argument list
src/FluxConvert.Core/Process/ProcessResult.cs          exit code, stdout, stderr, duration
src/FluxConvert.Core/Process/IProcessRunner.cs          runner contract
src/FluxConvert.Core/Process/ProcessRunner.cs           async runner with timeout + cancellation
src/FluxConvert.Core/Process/ExternalToolException.cs   "the tool could not be started"
src/FluxConvert.Core/Tools/ExternalToolKind.cs          ffmpeg / ffprobe / imagemagick
src/FluxConvert.Core/Tools/ExternalTool.cs              found-or-missing result with a reason
src/FluxConvert.Core/Tools/IToolLocator.cs              locator contract
src/FluxConvert.Core/Tools/ToolSearchContext.cs         where to look, and the user's overrides
src/FluxConvert.Core/Tools/ToolLocator.cs               the search itself
tests/FluxConvert.Core.Tests/ProcessRunnerTests.cs      8 tests (real processes)
tests/FluxConvert.Core.Tests/ToolLocatorTests.cs        8 tests
```

**Behaviour**

- `ProcessRunner` passes arguments through `ProcessStartInfo.ArgumentList`, so no shell is ever
  involved and a path with spaces, `&` or non-ASCII characters cannot be re-interpreted. stdout and
  stderr are drained concurrently, so a chatty tool cannot deadlock on a full pipe. A timeout
  terminates the whole process tree; cancellation does the same and rethrows
  `OperationCanceledException`.
- `ToolLocator` searches, in order: the explicit path from the settings → a bundled copy under
  the application folder, its `tools` folder, the repository root and the repository's `tools`
  folder → the system `PATH`. The first match wins.
- A missing program is reported as *unavailable* with a plain-English reason. The locator never
  throws.
- The version is read once with `<tool> -version` and cached together with the path. `Invalidate()`
  clears the cache.
- ImageMagick is only ever resolved as `magick.exe`. The legacy `convert.exe` name is deliberately
  ignored because on Windows it refers to the built-in file system conversion utility.

**Build:** `dotnet build FluxConvert.sln -c Debug` → *Build succeeded, 0 Warning(s), 0 Error(s)*.

**Tests:** `dotnet test FluxConvert.sln -c Debug` → *Failed: 0, Passed: 43, Skipped: 0*.

**Manual verification.** The application was launched again after the container registrations
changed; it started, showed its window and shut down cleanly, and the container passed
`ValidateOnBuild` with the new registrations.

**Issues found and fixed during this feature**

- `FluxConvert.Core.Process` shadows `System.Diagnostics.Process` inside that namespace. Both the
  runner and the tests now use an explicit `DiagnosticsProcess` alias.
- The relative search paths used `/` separators, which produced mixed-separator paths on Windows.
  They are now composed with `Path.Combine`.
- `CA1861` / `CA1859` required the version argument array and the `PATH` reader to avoid
  re-allocating a constant array on every call.

**Note for later phases.** `ProcessRunner` deliberately reports *what* the tool said rather than
interpreting it. Parsing FFmpeg's progress output and ImageMagick's stderr is a separate concern
and is scheduled with the respective engines (F-402, F-601).

---

### F-104 · Application shell: notification-area icon, single instance, graceful shutdown — **Verified**

**Created**

```text
src/FluxConvert.Core/Shell/SingleInstanceCoordinator.cs   named mutex + activation event
src/FluxConvert.Core/Shell/IConversionActivity.cs          "is a conversion running?"
src/FluxConvert.Core/Shell/ConversionActivityTracker.cs   thread-safe counter
src/FluxConvert.App/Shell/AppIconFactory.cs                the icon, drawn in code
src/FluxConvert.App/Shell/TrayIconController.cs            notification-area icon and menu
tests/FluxConvert.Core.Tests/SingleInstanceCoordinatorTests.cs   6 tests
tests/FluxConvert.Core.Tests/ConversionActivityTrackerTests.cs   5 tests
```

**Modified**

```text
src/FluxConvert.App/App.xaml.cs              owns the shell, the tray and the single-instance guard
src/FluxConvert.App/ShellWindow.xaml(.cs)    no taskbar button; closing hides the window
src/FluxConvert.App/app.manifest             Per-Monitor-V2 DPI + long-path awareness
src/FluxConvert.App/FluxConvert.App.csproj   UseWindowsForms, implicit using cleanup
src/FluxConvert.Core/Settings/AppSettings.cs StartMinimizedToTray now defaults to false
```

**Behaviour**

- The application lives in the notification area. It has no taskbar button, and closing the
  visible window hides it instead of ending the process. *Exit* in the tray menu really quits.
- A second launch does not start a second process. It signals the running instance, which brings
  its interface back to the front, and the second copy exits immediately.
- Quitting while a conversion is running asks for confirmation first. The check goes through
  `IConversionActivity`, which the conversion queue will register itself with.
- The tray icon and the application icon are drawn in code from the colour tokens, so no image
  file has to be shipped and the mark always matches the theme.

**Build:** `dotnet build FluxConvert.sln -c Debug` → *Build succeeded, 0 Warning(s), 0 Error(s)*.

**Tests:** `dotnet test FluxConvert.sln -c Debug` → *Failed: 0, Passed: 54, Skipped: 0*.

**Manual verification** (measured against the running process with Win32 calls):

| Check | Result |
| --- | --- |
| Process DPI awareness | `PER_MONITOR_DPI_AWARE` |
| Shell window visible at startup | yes |
| `WM_CLOSE` on the shell window | window hidden, process still alive |
| Second launch | second process exited, first process brought its window back |
| Notification-area icon | the icon's helper windows exist in the process; the taskbar on this machine is auto-hidden, so the tray area could not be photographed |

**Issues found and fixed during this feature**

- Enabling Windows Forms made `Application`, `Color` and `Pen` ambiguous with their WPF
  counterparts. The implicit WinForms and GDI+ usings are removed from the project
  (`<Using Remove="System.Windows.Forms" />`) and imported explicitly where they are needed.
- `WFAC010` asks for DPI awareness to be set through `ApplicationHighDpiMode` rather than the
  manifest. That property only configures Windows Forms *form* scaling, which this application
  does not use, and it did **not** produce a Per-Monitor-V2 manifest — the process was measured as
  `SYSTEM_DPI_AWARE`. The manifest entries were restored and the warning suppressed with the
  reason recorded in `app.manifest` and in the project file.
- **A real bug in the single-instance guard.** A named mutex is re-entrant for the thread that
  already owns it, so a second coordinator created on the same thread acquired the mutex
  successfully and wrongly claimed the primary role. A process-wide claim now backs the mutex up,
  and the failing test is what exposed it.
- `RegisteredWaitHandle` has no `Dispose`; the activation listener now uses `Unregister`.

---

### F-105 · `--diagnostics` command-line mode — **Verified**

**Created**

```text
src/FluxConvert.Core/Diagnostics/DiagnosticsEntry.cs       one line of the report
src/FluxConvert.Core/Diagnostics/DiagnosticsReport.cs      the report model and its text format
src/FluxConvert.Core/Diagnostics/DiagnosticsReportBuilder.cs builds the report from app state
src/FluxConvert.App/Diagnostics/ConsoleOutput.cs            console attach / allocate
src/FluxConvert.App/Diagnostics/DiagnosticsCommand.cs       the switches themselves
tests/FluxConvert.Core.Tests/DiagnosticsReportTests.cs      9 tests
```

**Modified**

```text
src/FluxConvert.Core/ProductInfo.cs    adds ProductInfo.Version from the assembly metadata
src/FluxConvert.App/App.xaml.cs         handles the switches before the single-instance check
```

**Behaviour**

- `FluxConvert.App.exe --diagnostics` prints a plain-text report and exits with code 0 without
  opening a window. The report has four sections: *System* (OS, architecture, runtime,
  application folder), *Paths* (data root, settings file, log directory, temporary directory),
  *Media tools* (FFmpeg, FFprobe and ImageMagick with their version and path, or the reason they
  are missing) and *Settings* (the effective preferences).
- `FluxConvert.App.exe --help` prints the usage text.
- The command-line modes are handled **before** the single-instance check, so a report can be
  produced while the application is already running. The running instance is not disturbed.
- The report is written to standard output, so it can be redirected to a file.

**Build:** `dotnet build FluxConvert.sln -c Debug` → *Build succeeded, 0 Warning(s), 0 Error(s)*.

**Tests:** `dotnet test FluxConvert.sln -c Debug` → *Failed: 0, Passed: 63, Skipped: 0*.

**Manual verification.** `FluxConvert.App.exe --diagnostics > diag.txt` produced a 34-line report
listing the data root, the log directory, the three media tools (all correctly reported as
unavailable with a reason) and the effective settings; the exit code was 0. `--help` printed the
usage text. With the application already running, `--diagnostics` still printed the report and
exited 0, and the running instance was unaffected.

**Issues found and fixed during this feature**

- **The report was written to the console instead of the redirected file.** `AttachConsole`
  replaces the process's standard handles, so re-opening standard output *after* attaching
  discarded the caller's redirection. `ConsoleOutput.Ensure` now leaves a redirected standard
  output completely alone and only attaches or allocates a console when there is nothing to
  preserve.
- `Console.Write(report)` bound to the `object` overload and printed the type name. The report is
  now formatted explicitly with `report.Format()`.

---

### F-201 · Design tokens, reusable styles and the original icon set — **Verified**

**Created**

```text
src/FluxConvert.App/Themes/Palette.xaml      colour tokens, gradients, shadow effects
src/FluxConvert.App/Themes/Typography.xaml   type scale and the Type.* text styles
src/FluxConvert.App/Themes/Spacing.xaml      spacing, radii, radial geometry, motion durations
src/FluxConvert.App/Themes/Icons.xaml        14 original vector icons
src/FluxConvert.App/Themes/Controls.xaml     item badge, core disc, detail strip, card, ghost button
src/FluxConvert.App/Controls/ProgressRing.cs determinate and indeterminate progress ring
src/FluxConvert.App/Controls/RadialItem.cs   the IsSelected attached property
src/FluxConvert.App/Converters/HexToBrushConverter.cs
src/FluxConvert.App/Converters/ResourceKeyToGeometryConverter.cs
src/FluxConvert.App/Swatch.cs                preview model
src/FluxConvert.App/IconEntry.cs             preview model
```

**Modified**

```text
src/FluxConvert.App/App.xaml                 merges the five theme dictionaries
src/FluxConvert.App/ShellWindow.xaml(.cs)    now shows the design-system preview
Directory.Build.props                         globalization is no longer invariant
```

**What the tokens contain**

- **Palette** — 11 surfaces, 2 borders, 3 text colours, 4 category accents, 5 state colours, 2
  gradients and 3 shadow effects. No view contains a hex value.
- **Typography** — 8 sizes and 8 named text styles (`Type.Display`, `Type.Title`, `Type.Subtitle`,
  `Type.Body`, `Type.Label`, `Type.Caption`, `Type.Overline`, `Type.Metric`).
- **Spacing** — a spacing scale, four corner radii, the radial geometry (ring radii, item radius,
  core disc diameter, detail strip size) and the motion durations from the art direction.
- **Icons** — 14 glyphs on a shared 24 × 24 grid with a 1.75 unit stroke, round caps and round
  joins: Image, Video, Audio, Settings, three chevrons, Close, Check, Folder, History, Queue,
  Retry, Warning, Error and Search. The path data is original artwork; no icon package is used.
- **Styles** — the radial item badge with its four states, the core disc, the detail strip, a card
  and a ghost button. The badge is a real `Button`, so focus, keyboard navigation and screen
  readers come from the framework; the *selected* state uses an attached property because a
  button has none of its own.

**Build:** `dotnet build FluxConvert.sln -c Debug` → *Build succeeded, 0 Warning(s), 0 Error(s)*.

**Tests:** `dotnet test FluxConvert.sln -c Debug` → *Failed: 0, Passed: 63, Skipped: 0*. No unit
test is meaningful for XAML resources; the verification is the build plus a screenshot.

**Manual verification.** The running application shows the whole design system. The palette
renders every token with its name and value, the type scale renders every role, the icon set
renders all 14 glyphs in badges, the badge states section shows rest, focused, selected,
disabled and submenu side by side, and the widget section shows the core disc, a determinate
progress ring at 35 %, an indeterminate ring and the detail strip. Two screenshots were captured
from the live window and inspected.

**Issues found and fixed during this feature**

- **A start-up crash caused by `InvariantGlobalization`.** The property made WPF unable to resolve
  the `en-US` culture, so every binding threw `Cannot find non-neutral culture related to
  'en-us'` the moment the first template was applied and the application died with exit code
  `-532462766`. Globalization is no longer invariant — a desktop application has to format
  numbers and dates the way the user's locale expects anyway. `CA1305` then required an explicit
  `CultureInfo` in the diagnostics report.
- `FontSize`, `FontWeight` and `x:Double` are not usable as XAML elements in this setup; the
  theme dictionaries use `sys:Double` and literal font weights instead.
- A trigger cannot target an element that lives inside a `RenderTransform`. The hover scale is
  now set by replacing the whole transform.

---

### F-202 · The radial ring prototype — **Verified**

**Created**

```text
src/FluxConvert.Core/Layout/RadialGeometry.cs       pure geometry: item positions, work-area clamping
src/FluxConvert.App/Radial/RingSurface.cs           the custom-drawn glass ring
src/FluxConvert.App/Radial/WorkAreaProvider.cs      the work area of the monitor under the pointer
src/FluxConvert.App/Radial/RadialWindow.xaml(.cs)   the floating widget
src/FluxConvert.App/ViewModels/RadialItemViewModel.cs
src/FluxConvert.App/ViewModels/RadialMenuViewModel.cs
tests/FluxConvert.Core.Tests/RadialGeometryTests.cs 13 tests
```

**Modified**

```text
src/FluxConvert.App/App.xaml.cs                   the widget replaces the design preview as the main UI
src/FluxConvert.App/Shell/TrayIconController.cs   a "Design preview" entry
src/FluxConvert.App/ShellWindow.xaml(.cs)         retitled, it is now the design preview
src/FluxConvert.App/Themes/Palette.xaml           the core disc is a little lighter than the ring
```

**What it does**

- A borderless, transparent, always-on-top window, 348 × 416 device-independent pixels, that
  never takes the focus when it appears.
- `RingSurface` paints the annulus as a single shape (the outer circle minus the inner circle),
  so the inner edge stays crisp instead of leaving the halo two overlapping ellipses would.
- Four real buttons sit on the ring at the documented radii: Images (magenta), Video (violet),
  Audio (teal) and More options. Their positions come from `RadialGeometry`, which is pure and
  unit tested.
- The core disc carries the product name and a hint; the detail strip below the ring describes
  the item the pointer is over.
- The widget is placed in the bottom right corner of the work area of the monitor that holds the
  pointer, and is clamped so that it can never end up partly off-screen. The work area is read
  with `MonitorFromPoint`/`GetMonitorInfo` and converted to device-independent pixels with that
  monitor's effective DPI, so the widget keeps the same physical size on a mixed-DPI setup.

**Build:** `dotnet build FluxConvert.sln -c Debug` → *Build succeeded, 0 Warning(s), 0 Error(s)*.

**Tests:** `dotnet test FluxConvert.sln -c Debug` → *Failed: 0, Passed: 76, Skipped: 0*.

**Manual verification**

| Check | Result |
| --- | --- |
| Widget size | 435 × 520 physical pixels = 348 × 416 DIP at 125 % scaling — exactly as designed |
| Position | 1172, 384 DIP — the bottom right of the 1536 × 816 work area with a 16 DIP margin |
| Focus | the foreground window is unchanged after the widget appears, so it never steals focus |
| Hover | moving the pointer over the Images badge changes the detail strip to *Images — Convert to PNG, JPEG, WEBP, AVIF, BMP, TIFF, GIF or ICO* |

Two screenshots of the live widget were captured and inspected against the reference: a
translucent glass ring with a large open centre, an opaque core disc inside it, four rounded
square badges evenly spaced on the ring, and the detail strip below.

**Issues found and fixed during this feature**

- `EventSetter` cannot resolve a directly-routed event such as `MouseEnter` by name, which
  crashed the application at start-up with a `XamlParseException`. The plain CLR event attributes
  on the button are used instead.
- **The first screenshots were wrong, not the widget.** The capture script was not per-monitor
  DPI aware, so `GetWindowRect` returned virtualised coordinates and `PrintWindow` clipped the
  widget: the ring looked oversized and the detail strip appeared to be missing. The capture
  script now enables per-monitor V2 awareness before it touches any window, and the widget is
  exactly the size it should be.
- The core disc was a shade darker than the ring, which inverted the relationship of the
  reference (a solid disc inside translucent glass). It is now slightly lighter.

---

## Next up

**F-203 · Item interaction model.**

- Acceptance criteria: arrow keys move the selection around the ring, Enter activates the focused
  item, Escape closes the widget, the hovered and focused items are visually distinct, and a
  disabled item explains why it cannot be used.
- Recommended tests: manual verification of the keyboard behaviour; the selection logic is a
  pure function and can be unit tested.
