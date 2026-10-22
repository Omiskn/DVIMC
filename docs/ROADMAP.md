# Roadmap

Status legend: **Planned** · **In Progress** · **Implemented** · **Verified** · **Blocked**

Every feature is a separate, independently verifiable work item with its own acceptance criteria
and its own Git commit. Features are not batched.

The authoritative record of what is actually done lives in [`../PROJECT_STATUS.md`](../PROJECT_STATUS.md)
and [`PROGRESS.md`](PROGRESS.md).

---

## Phase 0 — Repository and environment assessment

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-000 | Repository, Git and toolchain assessment | **Verified** | — |
| F-001 | Baseline documentation set + `PROJECT_STATUS.md` | **Verified** | F-000 |

**Outcome of F-000.** The repository contained only `.gitattributes` and an initial commit. The
installed SDK is **.NET 8.0.100**; FFmpeg, FFprobe and ImageMagick are **not installed**. The
machine has one display at 1920×1080 physical / 1536×864 logical (125 % scaling).

---

## Phase 1 — Application foundation

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-101 | Solution + project skeleton (`FluxConvert.Core`, `FluxConvert.App`, `FluxConvert.Core.Tests`) | **Verified** | F-001 |
| F-102 | Dependency injection container, structured logging, app data directories | **Verified** | F-101 |
| F-103 | Settings service (JSON, atomic write, defaults, validation) | Planned | F-102 |
| F-104 | Application shell: borderless main window, tray icon, graceful shutdown | **Verified** | F-102 |
| F-105 | `--diagnostics` command line mode (tool discovery report) | **Verified** | F-102 |

**Exit criteria.** `dotnet build` succeeds with zero warnings, `dotnet test` passes, and the app
launches to a visible shell that can be closed without a taskbar residue.

---

## Phase 2 — Visual foundation and the radial interface

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-201 | Design tokens (palette, typography, spacing), reusable styles, original icon set | **Verified** | F-101 |
| F-202 | Radial ring prototype: glass ring, solid core, evenly distributed item badges | **Verified** | F-201 |
| F-203 | Item interaction model: hover, selection, disabled, keyboard navigation, Escape to close | Planned | F-202 |
| F-204 | Open/close animation, category → format drill-down transition, reduced-motion setting | Planned | F-203 |
| F-205 | Screen-edge positioning and multi-monitor / DPI correctness | Planned | F-202 |

**Exit criteria.** The radial menu is a *real interactive control* (not decorative shapes): every
item is focusable, keyboard-navigable, and screen-reader labelled.

---

## Phase3 — Drag-and-drop workflow

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-301 | Accept files dropped from File Explorer on the floating target | Planned | F-202 |
| F-302 | File detection service (image / video / audio / unsupported, by extension + content probe) | Planned | F-101 |
| F-303 | Selected-file display in the core disc (name, type, size) | Planned | F-301, F-302 |
| F-304 | Multi-file drop handling and per-file feedback | Planned | F-301 |
| F-305 | Clear, actionable rejection of unsupported input | Planned | F-302 |

---

## Phase 4 — Image conversion vertical slice

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-401 | ~~External tool locator~~ — delivered early as **F-103**, because the version probe and the process runner it needs are prerequisites for the conversion engines | **Verified** (F-103) | F-102 |
| F-402 | ImageMagick capability detection (supported formats, delegates) | Planned | F-401 |
| F-403 | First end-to-end image conversion: PNG → WEBP | Planned | F-402 |
| F-404 | Output naming, collision protection, temporary file + atomic finalize | Planned | F-403 |
| F-405 | Output validation and user-facing error mapping | Planned | F-403 |

---

## Phase 5 — Complete image workflows

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-501 | Remaining image formats (JPEG, AVIF, BMP, TIFF, GIF, ICO) | Planned | F-403 |
| F-502 | Quality selection and quality presets | Planned | F-403 |
| F-503 | Resize with optional aspect-ratio preservation | Planned | F-403 |
| F-504 | Transparency and animation fidelity handling + warnings | Planned | F-501 |
| F-505 | Metadata privacy defaults (strip EXIF by default) | Planned | F-403 |
| F-506 | Image metadata preview in the detail strip | Planned | F-403 |

---

## Phase 6 — Audio conversion

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-601 | FFmpeg / FFprobe services with safe argument handling | Planned | F-401 |
| F-602 | Audio metadata inspection (duration, codec, bitrate, sample rate, channels) | Planned | F-601 |
| F-603 | First end-to-end audio path (WAV → MP3) | Planned | F-602 |
| F-604 | Format-specific settings (bitrate, sample rate, channels) | Planned | F-603 |
| F-605 | Progress reporting, cancellation, output validation | Planned | F-603 |
| F-606 | Remaining audio formats (FLAC, AAC, OGG, M4A, WMA) | Planned | F-603 |
| F-607 | Audio extraction from video | Planned | F-603 |

---

## Phase 7 — Video conversion

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-701 | Video metadata inspection (resolution, fps, codecs, duration, streams) | Planned | F-601 |
| F-702 | First end-to-end video path (MP4 → MP4 with re-encode) | Planned | F-701 |
| F-703 | Codec-aware conversion (H.264, H.265, VP9, AV1 — availability permitting) | Planned | F-702 |
| F-704 | Quality and resolution presets | Planned | F-702 |
| F-705 | Accurate progress reporting from FFmpeg output + time remaining | Planned | F-702 |
| F-706 | Stream copy when valid; audio track preservation; explicit warnings | Planned | F-703 |
| F-707 | Remaining containers (MKV, MOV, AVI, WEBM) and video → GIF | Planned | F-703 |

---

## Phase 8 — Queue and supporting features

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-801 | Conversion queue with queued/running/completed/cancelled/failed states | Planned | F-405 |
| F-802 | Batch conversion and retry of failed jobs | Planned | F-801 |
| F-803 | Persistent conversion history | Planned | F-801 |
| F-804 | Output directory selection, open output, open containing folder | Planned | F-405 |
| F-805 | Completion notification and optional auto-open of output folder | Planned | F-801 |
| F-806 | Settings UI wired to the settings service | Planned | F-103, F-806→F-104 |
| F-807 | Global hotkey to show the floating interface | Planned | F-104 |
| F-808 | Favorites and recently used formats | Planned | F-802 |
| F-809 | Compact queue window | Planned | F-801 |

---

## Phase 9 — Hardening and packaging

| ID | Feature | Status | Depends on |
| --- | --- | --- | --- |
| F-901 | Edge-case hardening (Unicode paths, long paths, huge files, tool crashes) | Planned | Phase 6, 7 |
| F-902 | Third-party license audit and `THIRD-PARTY-NOTICES` | Planned | F-401 |
| F-903 | Self-contained `win-x64` publish pipeline | Planned | F-902 |
| F-904 | Clean-machine validation and troubleshooting guide | Planned | F-903 |

---

## Explicitly out of scope

Accounts, sign-in, telemetry, analytics, advertising, cloud conversion, online file uploads, and
any global keyboard hook or system-wide drag interception that is not demonstrably required.
