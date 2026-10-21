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
| **Current feature** | F-101 — Solution and project skeleton (not started) |
| **Overall completion** | ~3 % — documentation and repository scaffolding only; **no application code exists yet** |
| **Most recent verified milestone** | F-001 — Baseline documentation set and repository scaffolding |
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
| F-000 | Repository / Git / toolchain assessment | Recorded the repository state, toolchain, display metrics, missing media tools and the resulting architecture constraints. | ✅ | n/a | ✅ (manual inspection) | see § 8 |
| F-001 | Baseline documentation and scaffolding | `README.md`, `PROJECT_STATUS.md`, `docs/{ROADMAP,PROGRESS,ARCHITECTURE,TESTING,ART_DIRECTION}.md`, 3 ADRs, `.gitignore`, `.editorconfig`, `.gitattributes`, `Directory.Build.props`. | ✅ | n/a | ✅ (files reviewed) | see § 8 |

Nothing else is complete. **No conversion feature exists yet** — no format in the README table is
currently implemented.

---

## 4. Features In Progress

**None.** F-101 is the next feature and has not been started.

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
| **Most recent build** | None yet — no project exists. The `.gitignore` / `.editorconfig` / `Directory.Build.props` files were written but not compiled. |
| **Tests executed** | None. The xUnit project is created in F-101. |
| **Known failures / limitations** | FFmpeg, FFprobe and ImageMagick are not installed on this machine, so no real conversion has been attempted. Integration tests are designed to skip with a printed reason rather than fail. |
| **Last verified** | 2026-10-21 (repository/toolchain inspection) |

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
| *(this commit)* | `docs: add project baseline, roadmap and architecture documentation` | F-000 assessment + F-001 documentation and scaffolding | **Local only** |

Pushing to GitHub requires explicit user authorisation. Until then the commits remain local for
review in GitHub Desktop.

---

## 9. Next Steps

### Next feature: **F-101 — Solution and project skeleton**

**Goal.** A buildable, testable three-project solution with no UI complexity yet.

**Acceptance criteria**

1. `FluxConvert.sln` contains `src/FluxConvert.Core` (`net8.0`), `src/FluxConvert.App`
   (`net8.0-windows`, `UseWPF`), `tests/FluxConvert.Core.Tests` (`net8.0`, xUnit).
2. `dotnet restore`, `dotnet build` and `dotnet test` all succeed with **zero warnings**
   (`TreatWarningsAsErrors` is on).
3. `dotnet run --project src/FluxConvert.App` starts the app and shows a visible window that
   closes cleanly without a lingering process.
4. `ProductInfo` exposes the product name from a single source used by the window title and the
   tray tooltip.
5. No hard-coded absolute paths; no `packages.config`; no committed binaries.

**Recommended tests**

- Unit: `ProductInfo.ProductName` is non-empty and constant.
- Smoke: `dotnet test` green with the suite actually executing (not silently skipped).

**Prerequisites**

- None. The .NET 8 SDK is sufficient.

---

## 10. Instructions for Resuming Work

1. `git status` — confirm a clean tree on `main`.
2. Read this file, then `docs/PROGRESS.md` for the detailed log.
3. Check `docs/ROADMAP.md` for the status of the next feature (F-101).
4. Inspect `docs/adr/` for the three accepted decisions that constrain the implementation.

**Relevant source files today:** none — the repository currently contains documentation and build
configuration only. The first code will live in:

```text
src/FluxConvert.Core/          domain services (no WPF dependency)
src/FluxConvert.App/           WPF shell
tests/FluxConvert.Core.Tests/  xUnit suite
```

**Continuation checklist for F-101**

- [ ] `dotnet new sln -n FluxConvert`
- [ ] `dotnet new classlib -n FluxConvert.Core -o src/FluxConvert.Core -f net8.0`
- [ ] `dotnet new wpf -n FluxConvert.App -o src/FluxConvert.App -f net8.0-windows`
- [ ] `dotnet new xunit -n FluxConvert.Core.Tests -o tests/FluxConvert.Core.Tests -f net8.0`
- [ ] Add project references (`App` → `Core`, `Tests` → `Core`)
- [ ] Add `CommunityToolkit.Mvvm`, `Microsoft.Extensions.DependencyInjection`,
      `Microsoft.Extensions.Logging` package references
- [ ] Delete the template `Class1.cs`; add `ProductInfo`
- [ ] `dotnet build` → zero warnings; `dotnet test` → green
- [ ] Launch the app and confirm the window appears and closes
- [ ] Update this file, `docs/PROGRESS.md`, `docs/ROADMAP.md`
- [ ] Commit: `chore: initialize WPF application foundation`
