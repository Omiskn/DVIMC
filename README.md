# FluxConvert

**FluxConvert** is a lightweight Windows desktop file converter built around a floating radial
interface. Drag a file onto the app, pick a target format from a radial menu, and the conversion
runs locally — no account, no upload, no cloud round-trip.

> **Working name.** "FluxConvert" is a placeholder while branding is finalised. The product name is
> defined in exactly one place (`src/FluxConvert.Core/ProductInfo.cs`) plus the WPF assembly
> metadata, so a rename is a small, mechanical change.

---

## What it does (target scope)

| Category | Target formats | Engine |
| --- | --- | --- |
| Image | PNG, JPEG/JPG, WEBP, AVIF, BMP, TIFF, GIF, ICO | ImageMagick (`magick`) |
| Video | MP4, MKV, MOV, AVI, WEBM, GIF | FFmpeg / FFprobe |
| Audio | MP3, WAV, FLAC, AAC, OGG, M4A, WMA | FFmpeg / FFprobe |

The application is built in phases; the authoritative list of what is *actually implemented and
verified* lives in [`PROJECT_STATUS.md`](PROJECT_STATUS.md). A format is only counted as supported
when it has been implemented **and** verified by a real conversion, not merely listed in a menu.

---

## Prerequisites

| Requirement | Notes |
| --- | --- |
| Windows | Windows 10 1809 (17763) or newer, x64 |
| .NET SDK | 8.0 or newer. The project targets `net8.0-windows`. |
| FFmpeg + FFprobe | Required for video and audio conversion. Must be on `PATH` or placed in `tools/ffmpeg/bin`. |
| ImageMagick | Required for image conversion. Must be on `PATH` or placed in `tools/imagemagick`. |

The app never modifies system `PATH`. It resolves the external tools in this order:

1. An explicit path configured in **Settings → Advanced**.
2. A bundled copy in `tools/` next to the application (or the repository root during development).
3. The system `PATH`.

If a required tool is missing, the app reports it clearly instead of failing silently — see
[`docs/TESTING.md`](docs/TESTING.md) for the exact commands to verify a local install.

---

## Development setup

```powershell
git clone https://github.com/Omiskn/DVIMC.git
cd DVIMC
dotnet restore
dotnet build
```

Run the app:

```powershell
dotnet run --project src/FluxConvert.App
```

Run the tests:

```powershell
dotnet test
```

> Media-engine integration tests are **skipped automatically** when FFmpeg/ImageMagick are not
> present, so `dotnet test` stays green on a clean machine. The skip reason is printed.

### Fetching the media tools for local development

The binaries are not committed to this repository (their redistribution terms are tracked in
[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md#third-party-tooling)). To work on conversion
features locally, place them under `tools/` (already git-ignored):

```text
tools/
  ffmpeg/bin/ffmpeg.exe
  ffmpeg/bin/ffprobe.exe
  imagemagick/magick.exe
```

Verify the discovery:

```powershell
dotnet run --project src/FluxConvert.App -- --diagnostics
```

`--diagnostics` prints the data root, the log folder, the media tools it found (with versions and
paths) and the effective settings, then exits. `--help` prints the usage text.

---

## Project layout

```text
FluxConvert.sln
Directory.Build.props         Shared compiler/analyzer settings
src/
  FluxConvert.Core/           Domain logic: detection, capabilities, conversion, settings, history
  FluxConvert.App/            WPF shell: views, view models, radial menu, tray icon
tests/
  FluxConvert.Core.Tests/     Unit + integration tests for the core library
docs/
  ARCHITECTURE.md             Component map and key design decisions
  ART_DIRECTION.md            Visual reference analysis and design language
  ROADMAP.md                  Phase-by-phase feature plan
  PROGRESS.md                 Verified work log
  TESTING.md                  How to run and extend the test suite
```

---

## Documentation

| Document | Purpose |
| --- | --- |
| [`PROJECT_STATUS.md`](PROJECT_STATUS.md) | **Start here.** Canonical status of the project. |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Architecture, decisions, third-party tooling. |
| [`docs/ART_DIRECTION.md`](docs/ART_DIRECTION.md) | Visual reference analysis and design tokens. |
| [`docs/ROADMAP.md`](docs/ROADMAP.md) | Feature roadmap with dependencies. |
| [`docs/PROGRESS.md`](docs/PROGRESS.md) | Dated log of completed and verified work. |
| [`docs/TESTING.md`](docs/TESTING.md) | Test strategy, commands, manual validation steps. |

---

## Privacy

Everything runs on your machine. FluxConvert has no accounts, no telemetry, no analytics, and no
network calls during ordinary use. Settings and logs are stored under:

```text
%LOCALAPPDATA%\FluxConvert\
  settings.json                        user preferences
  logs\fluxconvert-YYYYMMDD.log        daily rolling application log
  temp\                                scratch space for in-progress conversions
  history.json                         conversion history (added with the queue feature)
```

Set `FLUXCONVERT_DATA_ROOT` to move that folder somewhere else — useful for a portable install or
for isolating a test run.

---

## License

Not yet specified. All third-party components and their licenses are catalogued in
[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md#third-party-tooling).
