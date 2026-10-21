# ADR-0003 — Real controls for radial menu items

**Status:** Accepted · **Date:** 2026-10-21

## Context

The radial menu must be a genuinely interactive component: keyboard navigable, screen-reader
labelled, and able to show hover, focus, selected and disabled states. It also has to look like a
premium piece of glass rather than a grid of buttons.

Two implementations were considered:

1. **One custom-drawn canvas.** A single `FrameworkElement` draws the ring and every item, and
   hit-testing is done manually with trigonometry.
2. **Custom drawing for the ring, real controls for the items.** `RingSurface` renders the glass
   annulus, the item badge backgrounds and the progress arc. Each item is a genuine WPF `Button`
   hosted in an overlay `Canvas` and positioned by the ring at runtime.

## Decision

Option 2.

## Consequences

- Keyboard navigation, focus visualisation, `AutomationProperties.Name`, tab order and
  screen-reader support come from the framework instead of being re-implemented.
- Animation of the badges is slightly more expensive than drawing them directly, but the item
  count is small (typically 4–12) so the difference is not measurable.
- The layout code must keep the control positions and the drawn ring in sync; both are driven by
  the same geometry provider so they cannot drift.
