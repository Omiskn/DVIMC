# Art Direction

This document records the analysis of the visual reference and the design language that results
from it. It is the source of truth for the look and feel of the floating interface.

---

## 1. Visual reference

**Reference:** <https://radial.appverge.net> — "Radial", a macOS radial (pie) menu and automation
tool published by AppVerge.

**When it was inspected:** 2026-10-21, from the public marketing site.

### 1.1 What was actually accessible

The reference is a product landing page for a paid macOS application (listed at EUR 24.99). The
public page exposes:

- A hero banner image that shows the radial menu itself (`/RadialBanner.jpg`).
- Feature cards that illustrate macro behaviour in context.
- Static marketing copy describing actions, chaining, and keyboard-shortcut simulation.

The live, interactive application was **not** available for inspection. The design team therefore
based this analysis on the rendered hero image, which is the only authoritative depiction of the
radial widget, plus the textual description of the interaction model.

**Assumption register** (things that could not be confirmed and are therefore treated as design
guidance, not as fact about the reference):

| Unknown | Assumption used for FluxConvert |
| --- | --- |
| Whether items show text labels inline | Icons only on the ring; the label appears in a tooltip / detail strip |
| Exact hover and press animation curves | Standard 150–220 ms ease-out transitions |
| Whether the ring expands a second level in place | Yes — FluxConvert drills from category ring → format ring in place |
| Whether the reference supports a drag target | Not applicable; FluxConvert adds drag-and-drop as its primary entry point |
| Sound / haptic feedback | Not implemented |

---

## 2. Observations from the reference

From the hero image, the radial menu has these defining characteristics:

1. **A circular donut.** The widget is a ring of glass with a large, open circular hole in the
   middle. The hole is roughly half the diameter of the whole widget — it is the dominant negative
   space, not a small hub.
2. **A solid core.** The hole is filled by a distinct, opaque, lighter circular disc that carries
   the product identity as muted, low-contrast text. It reads as a button, but it is visually
   calm — it is not the loudest thing on screen.
3. **Items live on the ring.** Action icons are distributed evenly around the ring's mid-radius.
   Each icon sits in a rounded-square badge, which gives it a physical, app-like presence against
   the soft glass.
4. **A chevron means "more".** Items that open a nested menu carry a small `›` affordance next to
   the icon. This is a lightweight, non-textual way of signalling depth.
5. **Translucency with restraint.** The ring is frosted and semi-transparent; background colour
   bleeds through it. The core disc is opaque. The contrast between the two is the main depth cue.
6. **A single soft shadow.** The whole widget floats on one broad, low-opacity drop shadow. There
   is no per-item glow, no neon, no layered bloom.
7. **Minimal chrome.** No window frame, no title bar, no panel edges. The widget *is* the
   interface.

### 2.1 What FluxConvert keeps, changes, and adds

| Aspect | Reference | FluxConvert |
| --- | --- | --- |
| Overall form | Circular donut + solid core | **Kept** — identical silhouette |
| Palette | Light frosted glass, magenta/violet artwork | **Changed** — premium dark navy/charcoal with restrained blue / violet / teal / magenta accents, per the product brief |
| Item content | App icons and shortcuts | **Changed** — category glyphs (Image, Video, Audio) then format glyphs (PNG, WEBP, …) |
| Centre | Product name in muted text | **Changed** — contextual: selected file name, file type, and live progress |
| Depth cue | `›` chevron on nested items | **Kept** — used for category → format drill-down |
| Entry | Trigger gesture / hotkey | **Changed** — drag-and-drop onto the floating target is the primary entry point |
| Item labels | Not visible in the still | **Added** — a detail strip below the ring shows the hovered item's name and description |

---

## 3. FluxConvert design language

### 3.1 Colour tokens

Dark theme (default). Values are WPF `Color` hex values, defined once in
`src/FluxConvert.App/Themes/Palette.xaml` as `SolidColorBrush` resources.

| Token | Value | Use |
| --- | --- | --- |
| `Surface.Base` | `#0B0F1A` | Window/ring backdrop base |
| `Surface.Elevated` | `#141A2B` | Core disc, cards |
| `Surface.Ring` | `#1B2236` @ 88% | Radial ring glass fill |
| `Surface.Item` | `#232C44` | Radial item badge fill |
| `Surface.ItemHover` | `#2E3A5C` | Radial item badge, hovered |
| `Border.Subtle` | `#2A3350` | 1 px hairline borders |
| `Text.Primary` | `#E8ECF7` | Titles, file names |
| `Text.Secondary` | `#9AA6C4` | Labels, metadata |
| `Text.Muted` | `#66739A` | Disabled, hints |
| `Accent.Blue` | `#4C8DFF` | Primary action, focus ring |
| `Accent.Violet` | `#8B6CF0` | Video category |
| `Accent.Teal` | `#2DD4BF` | Audio category |
| `Accent.Magenta` | `#F472B6` | Image category |
| `State.Success` | `#34D399` | Completed jobs |
| `State.Warning` | `#FBBF24` | Quality / fidelity warnings |
| `State.Error` | `#F87171` | Failed jobs, unsupported input |

Rules: no pure black, no pure white, accents are used for **category identity and state only**,
never as large fills. Gradients are limited to a single subtle vertical lift on the core disc.

### 3.2 Typography

| Role | Size | Weight |
| --- | --- | --- |
| Core disc title | 15 px | SemiBold |
| Core disc subtitle | 12 px | Regular |
| Radial item label | 12 px | Medium |
| Detail strip title | 13 px | SemiBold |
| Detail strip body | 12 px | Regular |
| Progress percentage | 20 px | SemiBold (tabular figures) |

Font family: `Segoe UI Variable Text` when available (Windows 11), falling back to `Segoe UI`.
Numerals use `Consolas` or tabular figures so progress does not jitter.

### 3.3 Geometry

The radial widget is a square, transparent, `Window` with `AllowsTransparency="True"`,
`Background="Transparent"`, `WindowStyle="None"`, `ResizeMode="NoResize"`, and
`Topmost="True"`.

| Element | Value |
| --- | --- |
| Ring diameter | 320 px at 100 % scaling (DIP) |
| Ring outer radius | 150 px |
| Ring inner radius | 96 px |
| Ring thickness | 54 px |
| Core disc radius | 82 px |
| Item badge size | 44 × 44 px, corner radius 12 px |
| Item centre radius | 123 px (mid-ring) |
| Item gap | Evenly distributed by count; minimum arc gap 44 px |
| Detail strip | 320 × 56 px, 12 px below the ring |
| Outer padding | 14 px for the drop shadow bleed |

### 3.4 Motion

| Transition | Duration | Easing |
| --- | --- | --- |
| Widget fade + scale in | 180 ms | `CubicEase` ease-out |
| Widget fade + scale out | 140 ms | `CubicEase` ease-in |
| Ring items stagger in | 22 ms per item, max 220 ms total | `CubicEase` ease-out |
| Ring → format ring transition | 220 ms | `CubicEase` ease-in-out |
| Item hover | 120 ms | `QuadEase` ease-out |
| Progress ring sweep | continuous 1.4 s loop | `Linear` |

All durations collapse to **0 ms** when the *Reduced motion* accessibility setting is enabled; the
state changes still happen, they simply are not animated. This is a first-class setting, not a
hack.

### 3.5 Interaction states

Every radial item has five visible states. No pair relies on colour alone:

| State | Treatment |
| --- | --- |
| Rest | `Surface.Item` fill, 1 px `Border.Subtle`, icon at 80 % opacity |
| Hover | `Surface.ItemHover` fill, icon at 100 %, badge scales to 1.06 |
| Keyboard focus | `Surface.ItemFocus` fill, **2 px `Accent.Blue` border hugging the badge** |
| Selected | `Surface.ItemFocus` fill, 1 px `Accent.Blue` edge, `Shadow.Selected` glow **and a separate 1.5 px halo ring 4 px outside the badge** |
| Disabled | 45 % opacity, `Text.Muted`, with the reason in the detail strip, the tooltip and the automation help text |

Focus and selection are different *shapes* on purpose: focus is a thick border on the badge,
selection is a second ring floating around it. When an item is both at once it reads as a double
ring, which is the correct signal: "this is where the keyboard is, and this is what is chosen".

### 3.5.1 Keyboard model

| Key | Action |
| --- | --- |
| `↑` `→` `↓` `←` | Focus the enabled item closest to that edge of the ring |
| `Tab` / `Shift+Tab` | Move through every enabled item in ring order |
| `Home` / `End` | Focus the first / last enabled item |
| `Enter` / `Space` | Select the focused (or hovered) item |
| `Escape` | Dismiss the widget and return the focus to the previous window |

The arrow keys are spatial rather than sequential because a radial menu invites pointing: `→`
means the item on the right, not "the next one". Sequential traversal is still available through
`Tab`. Disabled items are never candidates, so the keyboard cannot land on something unusable.

### 3.6 Typography of the interface

All user-facing text is English. Errors explain the cause in plain language and offer an expandable
*Technical details* section with the exact tool invocation, so a support conversation can be
actionable without exposing internal paths in the summary line.

---

## 4. Implementation status

| Item | Status |
| --- | --- |
| Reference inspection and this document | **Verified** 2026-10-21 |
| Design tokens in XAML (`Themes/Palette.xaml`, `Typography.xaml`, `Spacing.xaml`) | **Verified** 2026-10-22 |
| Original vector icon set (`Themes/Icons.xaml`, 14 glyphs) | **Verified** 2026-10-22 |
| Reusable styles (`Themes/Controls.xaml`): item badge, core disc, detail strip, card, ghost button | **Verified** 2026-10-22 |
| `ProgressRing` control (determinate + indeterminate) | **Verified** 2026-10-22 |
| Interactive states of the radial item badge (rest, hover, focused, selected, disabled; submenu preview) | **Verified** 2026-10-22 |
| Keyboard navigation, selection and focus restoration (F-203) | **Verified** 2026-10-22 |
| Static radial ring prototype | **Verified** 2026-10-22 (F-202) |
| Category → format drill-down transition | Planned (F-204) |
| Reduced-motion support | Planned (F-204) |
| Comparison against the reference after the full interface exists | Planned (F-205) |

### 4.1 Where the tokens live

| File | Contents |
| --- | --- |
| `src/FluxConvert.App/Themes/Palette.xaml` | Every colour, the gradients and the three shadow effects |
| `src/FluxConvert.App/Themes/Typography.xaml` | The type scale and the `Type.*` text styles |
| `src/FluxConvert.App/Themes/Spacing.xaml` | Spacing, radii, the radial geometry and the motion durations |
| `src/FluxConvert.App/Themes/Icons.xaml` | The original icon set as `Geometry` resources |
| `src/FluxConvert.App/Themes/Controls.xaml` | The reusable control styles |

The dictionaries are merged in `App.xaml` in dependency order: a dictionary may only reference
resources that were merged before it.

### 4.2 Observations from the first implementation

Building the tokens and styles against the reference surfaced two adjustments:

- **The ring has to be genuinely translucent, but the item badges must not be.** In the reference
  the ring is frosted glass and the icons sit on it in solid rounded-square badges. The badge
  therefore uses `Surface.Item` (opaque) rather than the translucent `Surface.Ring`, so the icons
  stay crisp against whatever is behind the widget.
- **The chevron needs its own contrast.** On the reference the `›` affordance is a thin mark on
  light glass. On the dark theme it is drawn in `Text.Secondary` and sits outside the badge, so it
  does not compete with the icon.

Update this section as the interface is built; the live version of this document is the record.
