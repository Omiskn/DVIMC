# Testing

## 1. Strategy

| Layer | What it covers | How it runs |
| --- | --- | --- |
| Unit | File classification, format compatibility, output naming, collisions, settings serialisation, queue state transitions, argument construction, error mapping, progress parsing | `dotnet test` — no external tools required |
| Integration | FFmpeg/FFprobe availability and real conversions, ImageMagick availability and real conversions, metadata inspection, cancellation, output validation, Unicode and spaced paths | `dotnet test` — **auto-skipped** when the tool is missing |
| Manual | Floating window behaviour, animation quality, drag-and-drop, focus handling, Windows scaling, tray integration | Manual checklist below |

Rules:

- A test is only reported as passing if it actually executed. Integration tests that skip report
  their skip reason in the console output; a skipped test is **not** a pass.
- Test media is generated in-process (synthetic PNG, silent WAV, 1-second colour video) so the
  repository carries no copyrighted assets.

## 2. Running the tests

```powershell
dotnet test                                   # everything
dotnet test tests/FluxConvert.Core.Tests      # a single project
dotnet test --filter "FullyQualifiedName~Conversion"   # by name
dotnet test --logger "console;verbosity=detailed"      # show skip reasons
```

## 3. Environment for integration tests

The integration tests need the real binaries:

```text
tools/ffmpeg/bin/ffmpeg.exe
tools/ffmpeg/bin/ffprobe.exe
tools/imagemagick/magick.exe
```

or on the system `PATH`. The locator follows the same order in tests as in the app
(settings override → `tools/` → `PATH`), so a local test run matches production behaviour.

To see exactly which tools were found:

```powershell
dotnet run --project src/FluxConvert.App -- --diagnostics
```

## 4. Manual validation checklist

### 4.1 Floating interface

- [ ] Widget opens near the tray icon and stays fully inside the visible desktop area.
- [ ] Widget stays fully inside the visible desktop area on a second monitor, and when dragged
      toward a screen edge.
- [ ] Hover, keyboard focus and selection are visually distinct from each other.
- [ ] `Tab` / `Shift+Tab` move focus between radial items; arrow keys also move selection.
- [ ] `Escape` closes the widget and returns focus to the previous application.
- [ ] Opening and closing the widget does not steal focus from the active application.
- [ ] With *Reduced motion* enabled, no animation plays but every state still changes.

### 4.2 Drag and drop

- [ ] Dragging one supported file from File Explorer opens the radial menu.
- [ ] The core disc shows the file name, detected category and file size.
- [ ] Dragging an unsupported file shows a clear explanation and does not open an empty menu.
- [ ] Dragging multiple files shows the count and converts all of them.

### 4.3 Conversion

- [ ] Conversion progress is monotonic and reaches 100 % before the item is marked complete.
- [ ] Cancelling a running conversion leaves no partial output file behind.
- [ ] Converting a file whose target name already exists produces `name (2).ext` and does not
      overwrite the original.
- [ ] Converting a file into the same folder as the source never overwrites the source.
- [ ] A file with spaces, non-ASCII characters and a long name converts successfully.

### 4.4 Reliability

- [ ] Renaming or removing the media binaries while the app runs produces a readable error, not a
      crash.
- [ ] Closing the app during a conversion asks for confirmation and shuts down cleanly.

## 5. Test asset generation

Synthetic assets are produced by the test fixtures themselves:

- **PNG** — written byte-for-byte with `System.Drawing`/WPF encoders, or via ImageMagick when
  available.
- **WAV** — a 0.5 s, 44.1 kHz, mono, 16-bit PCM header plus silence, generated in memory.
- **Video** — a 1 s solid-colour clip produced by FFmpeg's `lavfi` `color` source when FFmpeg is
  available.

## 6. Current coverage status

See [`../PROJECT_STATUS.md`](../PROJECT_STATUS.md) § 6 for the most recent executed run, including
the exact commands and the observed results.
