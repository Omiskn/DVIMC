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

## Next up

**F-103 · External tool locator (FFmpeg, FFprobe, ImageMagick).**

- Acceptance criteria: `IToolLocator` resolves each binary from an explicit settings path, then a
  bundled `tools/` folder, then the system `PATH`; a missing tool is reported as unavailable
  rather than throwing; unit tests cover every resolution branch with a fake file system layout.
- Recommended tests: explicit override wins; bundled copy wins over `PATH`; `PATH` is used as a
  last resort; a missing tool yields `null` with an explanation, not an exception.
