# PROJECT_STATUS

> **Canonical status record for FluxConvert.** Read this file first at the start of every
> development session, and update it in the same commit as any feature, verified milestone,
> architectural change or newly discovered blocker.
>
> Last updated: **2026-10-21** · Overall stage: **Phase 0 complete — foundation starting**

---

## 1. Project Overview

| | |
| --- | --- |
| **Project name** | FluxConvert (working name — branding not final; see `src/FluxConvert.Core/ProductInfo.cs`) |
| **Purpose** | A lightweight Windows desktop file converter built around a floating radial interface: drop a file on the widget, pick a format from the radial menu, convert locally. |
| **Target platform** | Windows 10 1809+ / Windows 11, x64 |
| **Stack** | C#, WPF, XAML, MVVM (`CommunityToolkit.Mvvm`), `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging`, `System.Text.Json`, FFmpeg/FFprobe for A/V, ImageMagick for images |
| **Target framework** | `net8.0-windows` (see ADR-0001) |
| **Stage** | Phase 0 (assessment + baseline) **complete**. Phase 1 (application foundation) **not started**. |

---

## 2. Current Project Status

| | |
| --- | --- |
| **Current phase** | Phase 1 — Application foundation |
| **Current feature** | F-101 — Solution and project skeleton (**Verified**) → next: F-102 |
| **Overall completion** | ~7 % — documentation plus a buildable, launchable WPF shell. **No conversion feature exists yet.** |
| **Most recent verified milestone** | F-101 — Solution and project skeleton (built with 0 warnings, 4/4 tests green, app launched and rendered) |
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
| F-101 | Solution and project skeleton | Three-project solution (`FluxConvert.Core` net8.0, `FluxConvert.App` net8.0-windows WPF, `FluxConvert.Core.Tests` xUnit), shared build props, per-monitor-V2 DPI manifest, colour-token dictionary, and a temporary shell window that launches and closes cleanly. | ✅ | ✅ 0 warnings | ✅ 4/4 xUnit tests green | see § 8 |

Nothing else is complete. **No conversion feature exists yet** — no format in the README table is
currently implemented.

---

## 4. Features In Progress

**None.** F-102 is the next feature and has not been started.

---

## 5. Planned Features

Full detail, dependencies and acceptance criteria: [`docs/ROADMAP.md`](docs/ROADMAP.md).

### Phase 1 — Application foundation
- **F-101** Solution + projects (`FluxConvert.Core`, `FluxConvert.App`, `FluxConvert.Core.Tests`)
- **F-102** DI container, structured logging, app data directories
- **F-103** Settings service (JSON, atomic write, defaults)
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
| **Most recent build** | `dotnet build FluxConvert.sln -c Debug` → **Build succeeded, 0 Warning(s), 0 Error(s)** (2026-10-21) |
| **Tests executed** | `dotnet test FluxConvert.sln -c Debug` → **Passed! Failed: 0, Passed: 4, Skipped: 0** (`ProductInfoTests`) |
| **Manual verification** | App launched from `bin\Debug\net8.0-windows\FluxConvert.App.exe`; process stayed alive, `MainWindowTitle` = `FluxConvert`, window rendered with the dark theme (screenshot captured), process terminated cleanly on request. |
| **Known failures / limitations** | FFmpeg, FFprobe and ImageMagick are not installed on this machine, so no real conversion has been attempted. Integration tests are designed to skip with a printed reason rather than fail. |
| **Last verified** | 2026-10-21 |

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
| `0fbe81e` | `docs: add project baseline, roadmap and architecture documentation` | F-000 assessment + F-001 documentation and scaffolding | **Local only** |
| *(this commit)* | `chore: initialize WPF application foundation` | F-101 — solution, three projects, build props, DPI manifest, colour tokens, placeholder shell, first tests | **Local only** |

Pushing to GitHub requires explicit user authorisation. Until then the commits remain local for
review in GitHub Desktop.

---

## 9. Next Steps

### Next feature: **F-102 — Dependency injection, structured logging, app data directories**

**Goal.** Replace the hard-coded startup path with a composition root that owns the application's
services and logging, so later features can depend on abstractions instead of statics.

**Acceptance criteria**

1. `App` builds a `ServiceProvider` at startup and resolves view models / services from it.
2. A file logger writes rolling daily files under `%LOCALAPPDATA%\FluxConvert\logs\`; the log
   level is configurable; a console logger is used in Debug builds only.
3. An `AppPaths` service resolves `%LOCALAPPDATA%\FluxConvert\` and creates it on demand.
4. The service provider is disposed on shutdown; no unobserved task exceptions.
5. Unit tests cover the path resolution (including the "override via environment variable" case)
   and the file logger's file naming and level filtering.
6. `dotnet build` → 0 warnings; `dotnet test` → all green.

**Recommended tests**

- Unit: `AppPaths` returns the expected root and creates the directory when missing.
- Unit: `FileLogger` writes a file named `fluxconvert-YYYYMMDD.log` and honours the minimum
  level.
- Integration: none yet — no external tool is involved.

**Prerequisites**

- F-101 (done).

---

## 10. Instructions for Resuming Work

1. `git status` — confirm a clean tree on `main`.
2. Read this file, then `docs/PROGRESS.md` for the detailed log.
3. Check `docs/ROADMAP.md` for the status of the next feature (F-102).
4. Inspect `docs/adr/` for the three accepted decisions that constrain the implementation.

**Relevant source files today**

```text
src/FluxConvert.Core/ProductInfo.cs            product identity (single source)
src/FluxConvert.App/App.xaml(.cs)               entry point, owns the composition root
src/FluxConvert.App/ShellWindow.xaml(.cs)      temporary shell, replaced by the radial widget
src/FluxConvert.App/Themes/Palette.xaml         colour tokens
src/FluxConvert.App/app.manifest                PerMonitorV2 DPI + long-path awareness
tests/FluxConvert.Core.Tests/ProductInfoTests.cs
```

> **WPF gotcha to remember.** The markup compiler only generates `Application.InitializeComponent()`
> when `App.xaml` has loadable content. `App.xaml` therefore merges `Themes/Palette.xaml`, and that
> file must never be emptied — otherwise the build fails with `CS0103: InitializeComponent`.

**Continuation checklist for F-102**

- [ ] `src/FluxConvert.Core/Infrastructure/AppPaths.cs` — resolve/create the app data root
- [ ] `src/FluxConvert.Core/Infrastructure/FileLoggerProvider.cs` — daily rolling file logger
- [ ] `src/FluxConvert.Core/Settings/AppSettings.cs` + `SettingsService.cs` (JSON, atomic write)
- [ ] `src/FluxConvert.App/Composition/AppServices.cs` — the DI composition root
- [ ] `App.OnStartup` builds the provider; `OnExit` disposes it
- [ ] Unit tests for `AppPaths` and the file logger
- [ ] `dotnet build` → 0 warnings; `dotnet test` → green
- [ ] Update this file, `docs/PROGRESS.md`, `docs/ROADMAP.md`
- [ ] Commit: `feat: add composition root, file logging and settings persistence`
