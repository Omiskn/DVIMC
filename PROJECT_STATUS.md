# PROJECT_STATUS

> **Canonical status record for FluxConvert.** Read this file first at the start of every
> development session, and update it in the same commit as any feature, verified milestone,
> architectural change or newly discovered blocker.
>
> Last updated: **2026-10-22** · Overall stage: **Phase 2 in progress — the radial interface is interactive**

---

## 1. Project Overview

| | |
| --- | --- |
| **Project name** | FluxConvert (working name — branding not final; see `src/FluxConvert.Core/ProductInfo.cs`) |
| **Purpose** | A lightweight Windows desktop file converter built around a floating radial interface: drop a file on the widget, pick a format from the radial menu, convert locally. |
| **Target platform** | Windows 10 1809+ / Windows 11, x64 |
| **Stack** | C#, WPF, XAML, MVVM (`CommunityToolkit.Mvvm`), `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging`, `System.Text.Json`, FFmpeg/FFprobe for A/V, ImageMagick for images |
| **Target framework** | `net8.0-windows` (see ADR-0001) |
| **Stage** | Phase 0 and Phase 1 **complete**. Phase 2 (visual foundation and the radial interface) **in progress** — the widget is interactive, but no conversion feature exists yet. |

---

## 2. Current Project Status

| | |
| --- | --- |
| **Current phase** | Phase 2 — Visual foundation and the radial interface |
| **Current feature** | F-203 — The item interaction model (**Verified**) → next: F-204, animation and the format drill-down |
| **Overall completion** | ~38 % — Phase 1 complete; the design system, the floating widget and its keyboard interaction are verified. **No conversion feature exists yet.** |
| **Most recent verified milestone** | F-203 — Item interaction model (0 warnings, 101/101 tests green; arrow keys, Home/End, Enter, Tab skipping a disabled item and Escape focus restoration verified key by key against the live widget, with screenshots of the three interaction states) |
| **Blockers** | None blocking. FFmpeg/FFprobe/ImageMagick are **not installed** on this machine; this is handled by runtime tool discovery (ADR-0002) and by gracefully skipping integration tests. |

### Environment facts (verified 2026-10-21)

- SDK: **.NET 8.0.100** (only). WindowsDesktop runtime 8.0.23 present.
- `ffmpeg`, `ffprobe`, `magick`: **not on `PATH`**.
- Display: 1920×1080 physical, 1536×864 logical, 125 % scaling (`AppliedDPI` = 120).
- Git: branch `main`, remote `https://github.com/Omiskn/DVIMC.git` (untouched), working tree clean
  before this work began. No pre-existing user changes existed.

---

## 3. Completed Features

| ID | Feature | What it does | Implemented | Built | Tested | Commit |
| --- | --- | --- | --- | --- | --- | --- |
| F-000 | Repository / Git / toolchain assessment | Recorded the repository state, toolchain, display metrics, missing media tools and the resulting architecture constraints. | ✅ | n/a | ✅ (manual inspection) | `0fbe81e` |
| F-001 | Baseline documentation and scaffolding | `README.md`, `PROJECT_STATUS.md`, `docs/{ROADMAP,PROGRESS,ARCHITECTURE,TESTING,ART_DIRECTION}.md`, 3 ADRs, `.gitignore`, `.editorconfig`, `.gitattributes`, `Directory.Build.props`. | ✅ | n/a | ✅ (files reviewed) | `0fbe81e` |
| F-101 | Solution and project skeleton | Three-project solution (`FluxConvert.Core` net8.0, `FluxConvert.App` net8.0-windows WPF, `FluxConvert.Core.Tests` xUnit), shared build props, per-monitor-V2 DPI manifest, colour-token dictionary, and a temporary shell window that launches and closes cleanly. | ✅ | ✅ 0 warnings | ✅ 4/4 xUnit tests green | `70bb970` |
| F-102 | Composition root, file logging, settings persistence | DI container (validated at build), `AppPaths` data-root resolution, daily rotating file logger, atomic JSON settings persistence with corrupt-file quarantine. | ✅ | ✅ 0 warnings | ✅ 27/27 xUnit tests green | `c692935` |
| F-103 | External media tool locator and process runner | `ProcessRunner` (argument-list based, no shell, concurrent output drain, timeout, cancellation, process-tree kill) and `ToolLocator` (settings → bundled → `PATH`, cached version probe, never throws). | ✅ | ✅ 0 warnings | ✅ 43/43 xUnit tests green | `29aff83` |
| F-104 | Notification-area shell, single instance, graceful shutdown | Tray icon with context menu, no taskbar button, hide-on-close, second-instance activation, quit confirmation while a conversion runs, code-drawn application icon. | ✅ | ✅ 0 warnings | ✅ 54/54 xUnit tests green + 4 live Win32 checks | `3cd3023` |
| F-105 | `--diagnostics` command-line mode | Plain-text report of the system, paths, media tools and effective settings; `--help` usage text; works while the application is already running. | ✅ | ✅ 0 warnings | ✅ 63/63 xUnit tests green + real command run | `3c52013` |
| F-201 | Design tokens, reusable styles, original icon set | Complete palette / type scale / spacing scale, 14 original vector icons, radial item badge with four states, core disc, detail strip, progress ring. | ✅ | ✅ 0 warnings | ✅ 63/63 tests green + 2 screenshots of the live app | `c2c8ce1` |
| F-202 | The radial ring prototype | Floating borderless transparent widget: custom-drawn glass ring, solid core disc, four real buttons on the ring, detail strip, work-area placement with DPI-correct clamping. | ✅ | ✅ 0 warnings | ✅ 76/76 tests green + live size/position/focus/hover checks + 2 screenshots | `ad6d860` |
| F-203 | Item interaction model | Spatial keyboard navigation (`RingSelection`, pure and unit tested), `Home`/`End`, `Enter`/`Space` activation, `Escape` dismissal that restores the previous foreground window, distinct hover / keyboard-focus / selection-halo states, and a disabled item that explains itself in the strip, the tooltip and to a screen reader. | ✅ | ✅ 0 warnings | ✅ 101/101 tests green + live key-by-key checks, Tab traversal, focus restoration + 4 screenshots | see § 8 |

Nothing else is complete. **No conversion feature exists yet** — no format in the README table is
currently implemented.

---

## 4. Features In Progress

**None.** F-203 is complete; F-204 is the next feature and has not been started.

---

## 5. Planned Features

Full detail, dependencies and acceptance criteria: [`docs/ROADMAP.md`](docs/ROADMAP.md).

### Phase 1 — Application foundation
- **F-101** Solution + projects (`FluxConvert.Core`, `FluxConvert.App`, `FluxConvert.Core.Tests`)
- **F-102** DI container, structured logging, app data directories, settings persistence
- **F-103** External media tool locator and process runner (delivered early; see the note in
  `docs/ROADMAP.md`)
- **F-104** Application shell: borderless main window, tray icon, graceful shutdown
- **F-105** `--diagnostics` command-line mode

### Phase 2 — Visual foundation and the radial interface
- **F-201** Design tokens · **F-202** Radial ring prototype · **F-203** Item interaction + keyboard ·
  **F-204** Animations + reduced motion · **F-205** Screen-edge positioning / DPI

### Phase 3 — Drag-and-drop workflow
- **F-301** Explorer drag-and-drop · **F-302** File detection service · **F-303** Selected-file display ·
  **F-304** Multi-file handling · **F-305** Unsupported-input errors

### Phase 4–5 — Image conversion
- **F-401** Tool locator · **F-402** ImageMagick capability detection · **F-403** PNG→WEBP end-to-end ·
  **F-404** Output naming / collision / atomic finalize · **F-405** Validation + error mapping ·
  **F-501** Remaining formats · **F-502** Quality · **F-503** Resize · **F-504** Transparency/animation ·
  **F-505** Metadata privacy · **F-506** Metadata preview

### Phase 6 — Audio conversion
- **F-601** FFmpeg/FFprobe services · **F-602** Audio metadata · **F-603** WAV→MP3 end-to-end ·
  **F-604** Format settings · **F-605** Progress/cancel/validation · **F-606** Remaining formats ·
  **F-607** Extraction from video

### Phase 7 — Video conversion
- **F-701** Video metadata · **F-702** First end-to-end video path · **F-703** Codec-aware conversion ·
  **F-704** Quality/resolution presets · **F-705** Accurate progress · **F-706** Stream copy + audio ·
  **F-707** Remaining containers + video→GIF

### Phase 8 — Queue and supporting features
- **F-801** Queue with job states · **F-802** Batch + retry · **F-803** History · **F-804** Output folder
  actions · **F-805** Notifications · **F-806** Settings UI · **F-807** Global hotkey ·
  **F-808** Favorites/recents · **F-809** Compact queue window

### Phase 9 — Hardening and packaging
- **F-901** Edge cases · **F-902** License audit + notices · **F-903** Self-contained `win-x64` publish ·
  **F-904** Clean-machine validation + troubleshooting

### Explicitly out of scope
Accounts, telemetry, analytics, advertising, cloud conversion, online uploads, global hooks.

---

## 6. Testing and Build Status

| | |
| --- | --- |
| **Most recent build** | `dotnet build FluxConvert.sln -c Debug` → **Build succeeded, 0 Warning(s), 0 Error(s)** (2026-10-22) |
| **Tests executed** | `dotnet test FluxConvert.sln -c Debug` → **Passed! Failed: 0, Passed: 101, Skipped: 0** (`ProductInfoTests`, `AppPathsTests`, `FileLoggerProviderTests`, `SettingsServiceTests`, `ProcessRunnerTests`, `ToolLocatorTests`, `SingleInstanceCoordinatorTests`, `ConversionActivityTrackerTests`, `DiagnosticsReportTests`, `RadialGeometryTests`, `RingSelectionTests`) |
| **Manual verification** | F-203 was driven key by key against the live widget through UI Automation: from no focus, `Right` → *Video*, `Down` → *Audio*, `Up` → *Images*; `End` → *Audio* and `Home` → *Images*; `Enter` selected *Images* (strip: *Images · selected*); `Tab` walked Images → Video → Audio → Images, so the disabled *More options* item was skipped; hovering it turned the strip amber with *Settings, queue and history are not available yet.*; `Escape` hid the window and returned the foreground to the exact window that held it before. The widget is still 435 × 520 physical pixels at 1465,480. Earlier checks: DPI awareness `PER_MONITOR_DPI_AWARE`, hide-on-close keeps the process alive, `--diagnostics` prints a 34-line report. |
| **Known failures / limitations** | FFmpeg, FFprobe and ImageMagick are **not installed** on this machine, so the locator reports them as unavailable and no real conversion has been attempted. Integration tests are designed to skip with a printed reason rather than fail. |
| **Last verified** | 2026-10-22 |

---

## 7. Technical Decisions

| # | Decision | Why |
| --- | --- | --- |
| 1 | Target `net8.0-windows`, not `net10.0-windows` | Only the .NET 8 SDK is installed here; upgrading the SDK was out of scope. Trivial to change later. (ADR-0001) |
| 2 | Media tools are **located at runtime**, never hard-coded | They are absent from this machine and their redistribution terms are unverified. (ADR-0002) |
| 3 | Radial items are **real WPF buttons**; only the ring is custom-drawn | Free keyboard navigation, focus handling and accessibility instead of hand-rolled hit-testing. (ADR-0003) |
| 4 | `FluxConvert.Core` has **no WPF dependency** | Keeps the domain logic unit-testable and fast to test. |
| 5 | `CommunityToolkit.Mvvm` source generators instead of a full MVVM framework | Maintained by Microsoft, source-generated, no runtime surprises. |
| 6 | In-house file logger instead of Serilog | One fewer dependency; the requirement is simple. |
| 7 | Original vector icon set authored in-repo | No third-party icon licensing questions; consistent geometry. |
| 8 | Conversions write to a temp file and are atomically finalized | A failed or cancelled conversion never leaves a broken output. |
| 9 | App data root is `%LOCALAPPDATA%\FluxConvert\` | Predictable, per-user, no admin rights needed. |
| 10 | Third-party binaries are **not** committed | Their licences have not been verified yet (F-902). |

---

## 8. Git History

| Commit | Message | Contents | Pushed? |
| --- | --- | --- | --- |
| `1738ae4` | `Initial commit` | `.gitattributes` only | Yes (pre-existing) |
| `0fbe81e` | `docs: add project baseline, roadmap and architecture documentation` | F-000 assessment + F-001 documentation and scaffolding | **Yes** |
| `70bb970` | `chore: initialize WPF application foundation` | F-101 — solution, three projects, build props, DPI manifest, colour tokens, placeholder shell, first tests | **Yes** |
| `c692935` | `feat: add composition root, file logging and settings persistence` | F-102 — DI container, `AppPaths`, file logger, atomic JSON settings with quarantine | **Yes** |
| `29aff83` | `feat: add external media tool locator and process runner` | F-103 — `ProcessRunner`, `ToolLocator`, `ToolSearchContext`, DI registrations, 16 new tests | **Yes** |
| `3cd3023` | `feat: add tray icon, single instance and graceful shutdown` | F-104 — notification-area shell, single-instance coordination, quit confirmation, code-drawn icon | **Yes** |
| `3c52013` | `feat: add diagnostics command line mode` | F-105 — `--diagnostics` report, `--help` usage, console handling, 9 new tests | **Yes** |
| `c2c8ce1` | `feat: add design tokens, reusable styles and icon set` | F-201 — palette, typography, spacing, 14 original icons, control styles, progress ring | **Yes** |
| `ad6d860` | `feat: add the radial ring prototype` | F-202 — floating widget, glass ring, core disc, item badges, work-area placement | **Yes** |
| `bd53a95` | `docs: mark the F-202 commit as pushed` | F-202 hash and push state in this table | **Yes** |
| *(this commit)* | `feat: add radial menu keyboard and selection interaction` | F-203 — `RingSelection`, view-model focus/selection state, key handling, focus restoration, distinct focus and selection visuals, disabled-item explanation | **Yes** |

Every commit above is on `origin/main`; each push was verified with
`git ls-remote origin refs/heads/main`. Further pushes need the user's explicit authorisation.

---

## 9. Next Steps

### Next feature: **F-204 — Animation and the format drill-down**

**Goal.** Make the widget feel alive and turn a chosen category into a real format ring, without
ever blocking input or ignoring the *Reduced motion* preference.

**Acceptance criteria**

1. The widget fades and scales in over 180 ms and out over 140 ms; items stagger in at 22 ms each
   with the total capped at 220 ms.
2. Activating a category transitions the ring to that category's formats over 220 ms. `Escape`
   steps back to the category ring; `Escape` on the category ring dismisses the widget and restores
   the previous window (the F-203 behaviour).
3. A persisted *Reduced motion* setting collapses every duration to 0 ms while every state change
   still happens. The setting takes effect without a restart.
4. Animations never block input: a key pressed during a transition is applied to the new ring, and
   keyboard focus is never left on an element that has been removed.
5. The ring-state machine (categories → formats → back) and the motion-duration resolver are pure
   and unit tested.

**Recommended tests**

- Unit: drill-down and back, including disabled formats and the focus target after each step.
- Unit: every motion duration resolves to zero when reduced motion is on.
- Manual: the full keyboard path with reduced motion both off and on, plus a screenshot mid-transition.

**Prerequisites**

- F-203 (done). The format catalogue can start as the static target-format table from the README;
  the engines attach to it later (F-402, F-601).

---

## 10. Instructions for Resuming Work

1. `git status` — confirm a clean tree on `main`.
2. Read this file, then `docs/PROGRESS.md` for the detailed log.
3. Check `docs/ROADMAP.md` for the status of the next feature (F-204).
4. Inspect `docs/adr/` for the accepted decisions that constrain the implementation.

**Relevant source files today**

```text
src/FluxConvert.Core/ProductInfo.cs                product identity (single source)
src/FluxConvert.Core/Infrastructure/AppPaths.cs    application data root
src/FluxConvert.Core/Infrastructure/FileLoggerProvider.cs
src/FluxConvert.Core/Settings/*                    persisted preferences
src/FluxConvert.Core/Process/*                     safe external process execution
src/FluxConvert.Core/Tools/*                       media tool discovery
src/FluxConvert.Core/Shell/*                       single instance, activity tracking
src/FluxConvert.Core/Diagnostics/*                 the diagnostics report
src/FluxConvert.Core/Layout/RadialGeometry.cs      pure layout maths (unit tested)
src/FluxConvert.Core/Layout/RingSelection.cs      pure keyboard selection maths (unit tested)
src/FluxConvert.App/Composition/AppServices.cs     DI composition root
src/FluxConvert.App/App.xaml(.cs)                  entry point and application shell
src/FluxConvert.App/Shell/*                        notification-area icon, drawn icon
src/FluxConvert.App/Diagnostics/*                  command-line modes
src/FluxConvert.App/Controls/*                     ProgressRing, RadialItem attached property
src/FluxConvert.App/Converters/*                   hex and resource-key converters
src/FluxConvert.App/Themes/*                       palette, typography, spacing, icons, controls
src/FluxConvert.App/Radial/*                       widget, ring surface, work area, foreground restore
src/FluxConvert.App/ViewModels/*                   radial menu and item view models
src/FluxConvert.App/ShellWindow.xaml(.cs)          the design-system preview
tests/FluxConvert.Core.Tests/                       xUnit suite
```

> **WPF gotcha to remember.** The markup compiler only generates `Application.InitializeComponent()`
> when `App.xaml` has loadable content. `App.xaml` therefore merges `Themes/Palette.xaml`, and that
> file must never be emptied — otherwise the build fails with `CS0103: InitializeComponent`.

> **Globalization gotcha.** `InvariantGlobalization` must stay off. WPF resolves the culture of
> every binding, and an invariant runtime has no `en-US` culture, so the interface throws at
> start-up.

> **Implicit usings gotcha.** WPF projects get a reduced implicit-usings set. `System.IO` is **not**
> included — add `using System.IO;` explicitly in App-project files that need it. Enabling Windows
> Forms adds `System.Windows.Forms` and `System.Drawing` implicitly, which makes `Application`,
> `Color` and `Pen` ambiguous; both are removed again with `<Using Remove="..." />`.

> **Namespace gotcha.** `FluxConvert.Core.Process` shadows `System.Diagnostics.Process` inside
> that namespace. Use the `DiagnosticsProcess` alias.

> **DPI gotcha.** `ApplicationHighDpiMode` does **not** write the DPI settings into the manifest.
> The manifest in `src/FluxConvert.App/app.manifest` declares Per-Monitor-V2 by hand and
> `WFAC010` is suppressed with that reason recorded.

> **Console gotcha.** `AttachConsole` replaces the standard handles. If standard output is already
> redirected to a file or a pipe, never re-open it — the redirection would be lost.

> **Screenshot gotcha.** A capture script must call `SetProcessDpiAwarenessContext(-4)` before it
> creates any window. Otherwise the coordinates from user32 are virtualised and the capture is
> clipped, which looks like a layout bug in the application.

> **Disabled-control gotcha.** A disabled WPF button raises no mouse events, so it cannot tell the
> detail strip why it is unavailable. Each badge therefore sits inside a hit-testable `Grid` that
> owns `MouseEnter`/`MouseLeave`; the button itself only owns `Click`.

> **Foreground gotcha.** `ShowActivated="False"` plus `Activate()` is not enough when the widget is
> summoned without recent input (for example by a second launch): Windows refuses the foreground
> change, silently. `ForegroundWindowTracker` captures the window the user came from and uses
> `AttachThreadInput` as the fallback when raising or restoring it.

> **Topmost gotcha.** `Topmost = true; Topmost = false;` leaves the window *not* topmost. The F-202
> `ShowShell` did exactly that. The widget is topmost through its XAML and the toggle was removed
> in F-203.

**Continuation checklist for F-204**

- [ ] `src/FluxConvert.Core/Layout/` — a pure category ↔ format ring state machine (unit tested)
- [ ] A static format catalogue in Core from the README target table, with per-format availability
- [ ] `RadialMenuViewModel` can swap the item set and keep focus on a valid item
- [ ] Open/close fade + scale (180 / 140 ms) and the 220 ms ring transition
- [ ] Item stagger-in (22 ms each, 220 ms cap)
- [ ] A persisted *Reduced motion* setting that zeroes every duration
- [ ] `Escape` steps back one level before dismissing
- [ ] `dotnet build` → 0 warnings; `dotnet test` → green
- [ ] Update this file, `docs/PROGRESS.md`, `docs/ROADMAP.md`, `docs/ART_DIRECTION.md`,
      `docs/TESTING.md`
- [ ] Commit: `feat: add ring animation and format drill-down`
