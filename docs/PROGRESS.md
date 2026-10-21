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

## Next up

**F-102 · Composition root, file logging and settings persistence.**

- Acceptance criteria: DI container owns the services; a daily rolling file logger writes to
  `%LOCALAPPDATA%\FluxConvert\logs\`; `AppPaths` creates the data root on demand; the provider is
  disposed on shutdown; unit tests cover path resolution and log-file naming/level filtering.
- Recommended tests: `AppPaths` root resolution and directory creation; file logger writes
  `fluxconvert-YYYYMMDD.log` and honours the minimum level.
