# ADR-0002 — Locate media tools at runtime

**Status:** Accepted · **Date:** 2026-10-21

## Context

Conversions depend on three external binaries: `ffmpeg.exe`, `ffprobe.exe` and `magick.exe`.
None of them is installed on the development machine, and their redistribution terms differ by
build (FFmpeg is GPL or LGPL depending on which libraries are enabled; ImageMagick's Windows
portable build ships under a different licence than the standard release).

The brief forbids bundling executables whose redistribution terms have not been verified.

## Decision

Introduce `IToolLocator`, which resolves each binary in this order:

1. An explicit path from application settings (the *Advanced* settings section).
2. A bundled copy in `Tools/` next to the application executable (or `tools/` in the repository
   root during development).
3. The system `PATH`.

No absolute path is hard-coded anywhere. `tools/` is git-ignored. The `tools/` directory is used
for local development only; no third-party executable is committed to the repository.

## Consequences

- The application builds, starts and reports its capabilities correctly on a machine with no
  media tools installed — the affected options are disabled with an explanation instead of
  failing at conversion time.
- Integration tests skip cleanly when the tools are absent, with the skip reason printed.
- Bundling the binaries into a release remains a separate, explicitly licensed step (F-902).
