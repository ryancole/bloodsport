# CAL — Design Tokens & Brand Reference

**Champions Amateur League (CAL)** — the Bloodsport site design language.

This is a **Tailwind CSS v4** system (`@import "tailwindcss"`). It uses Tailwind's
**default palette** rather than a custom theme, plus a thin component layer built with
`@apply` (see [`Styles/app.css`](../src/Websites/BloodsportSite/BloodsportSite/Styles/app.css)).
This document is a shareable snapshot of that language — colors, type, spacing, and the
reusable component classes — extracted from the source so it can be referenced or ported
elsewhere.

> **Note on hex values:** Tailwind v4 defines its palette in OKLCH. The hex values below are
> the standard sRGB equivalents of Tailwind's default palette; `#0ea5e9` (sky-500) and
> `#111827` (gray-900) are confirmed by literal use in the brand SVGs.

---

## Brand

**Name:** Champions Amateur League — abbreviated **CAL**.

**Logo:** an 8-bit / pixel-art "CAL" coin.
- Dark rounded-square field: **`#111827`** (gray-900), corner radius `6` on a 32×32 grid.
- Letters "CAL" rendered as **white** (`#ffffff`) 2×2 pixel blocks (`shape-rendering: crispEdges`).
- Coin base / plinth: two stacked **sky** bars in **`#0ea5e9`** (sky-500).
- Source: [`wwwroot/cal-logo.svg`](../src/Websites/BloodsportSite/BloodsportSite/wwwroot/cal-logo.svg)
  (navbar mark) and [`wwwroot/favicon.svg`](../src/Websites/BloodsportSite/BloodsportSite/wwwroot/favicon.svg)
  (text variant).

**Logo treatment:** in the navbar the mark renders at `h-8 w-8`; on hover it gets a sky glow:
`drop-shadow(0 0 8px rgba(56,189,248,0.7))` (56,189,248 = sky-400), 200ms transition.

---

## Color

### Primary accent — Brand (sky)
The single brand accent, exposed as the **`brand-*`** scale via a Tailwind `@theme` block in
[`Styles/app.css`](../src/Websites/BloodsportSite/BloodsportSite/Styles/app.css) — use
`bg-brand-600`, `text-brand-500`, etc. The values mirror Tailwind's `sky` palette; changing the
`--color-brand-*` variables re-skins the whole app in one place. Used for links, primary buttons,
focus rings, active nav state, page-title/section-title accents, and the info tone.

| Token (`brand-*`) | Hex | Usage |
|---|---|---|
| brand-50  | `#f0f9ff` | info alert bg |
| brand-100 | `#e0f2fe` | pill / badge bg (info) |
| brand-200 | `#bae6fd` | info alert border |
| brand-300 | `#7dd3fc` | active nav text (on dark), pill dot |
| brand-400 | `#38bdf8` | logo hover glow, navbar brand hover |
| brand-500 | `#0ea5e9` | **primary brand** — heading accents, logo coin, focus ring |
| brand-600 | `#0284c7` | primary button, links |
| brand-700 | `#0369a1` | primary button hover, link hover |
| brand-800 | `#075985` | info alert text |
| brand-900 | `#0c4a6e` | stat-card figure (Home) |

### Neutrals — Gray
Text, surfaces, borders, and the dark navigation shell.

| Token | Hex | Usage |
|---|---|---|
| gray-50  | `#f9fafb` | table row hover, secondary-button hover |
| gray-100 | `#f3f4f6` | subtle fills, avatar placeholder bg, table cell border |
| gray-200 | `#e5e7eb` | card / divider borders |
| gray-300 | `#d1d5db` | input borders, nav text (on dark) |
| gray-400 | `#9ca3af` | muted / placeholder text |
| gray-500 | `#6b7280` | secondary text, eyebrow, table headers |
| gray-600 | `#4b5563` | body-secondary text |
| gray-700 | `#374151` | body text, table cells, secondary-button text |
| gray-800 | `#1f2937` | navbar borders/dividers (on dark) |
| gray-900 | `#111827` | **navbar background**, headings, logo field |

### Semantic tones
Each tone follows the same shape: `-50/-100` background, `-200` border, `-600/-700` text/solid.

| Tone | Meaning | Key shades |
|---|---|---|
| **Green**  | success / win  | bg green-50/100, border green-200, text green-600/700 |
| **Amber**  | warning / pending | bg amber-50/100, border amber-200, text amber-700/900 |
| **Red**    | danger / loss / destructive | bg red-50/100, border red-200, solid red-600 → hover red-700 |
| **Sky**    | info | bg sky-50/100, border sky-200, text sky-800 |
| **Emerald**| accent stat (Home) | text emerald-700/900 |

Common hexes: green-600 `#16a34a`, green-700 `#15803d`, amber-700 `#b45309`,
red-600 `#dc2626`, red-700 `#b91c1c`, emerald-700 `#047857`.

---

## Typography

**Font family:** Tailwind's default `font-sans` stack (no custom webfont is loaded) —
`ui-sans-serif, system-ui, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", …`.

| Role | Classes | Notes |
|---|---|---|
| Page title | `text-2xl font-bold text-gray-900` | + 3px × 2.5rem sky-500 underline accent (`::after`) |
| Section title | `text-lg font-semibold text-gray-900` | + 3px sky-500 left border, `pl-2.5` |
| Eyebrow | `text-xs font-semibold uppercase tracking-wider text-gray-500` | overline label |
| Body | `text-sm` (default `leading-relaxed` in docs) | gray-700 |
| Table header | `text-xs font-semibold uppercase tracking-wider text-gray-500` | |
| Doc H2 / H3 | `text-xl font-bold` / `text-base font-semibold` | long-form pages |

---

## Spacing, radius & elevation

| Property | Value | Where |
|---|---|---|
| Page container | `mx-auto max-w-6xl px-6 py-6` | `.page-wrap` |
| Doc width | `max-w-3xl` | `.doc` |
| Card padding | `p-5` | `.card` |
| Card radius | `rounded-lg` (0.5rem) | cards / panels |
| Button radius | `rounded-md` (0.375rem) | `.btn`, `.input` |
| Pill / badge radius | `rounded-full` | `.badge`, pills |
| Elevation | `shadow-sm` | cards, inputs |
| Navbar height | `h-14` (3.5rem), sticky, `z-40` | `TopNav` |

---

## Component classes

Defined in `@layer components` in
[`Styles/app.css`](../src/Websites/BloodsportSite/BloodsportSite/Styles/app.css) — the reusable
primitives shared across ~50 Blazor components.

| Class | Purpose |
|---|---|
| `.page-wrap` | centered page container (max-w-6xl) |
| `.page-title` | H1 with sky underline accent |
| `.section-title` | H2 with sky left-border accent |
| `.eyebrow` | uppercase overline label |
| `.card` | white rounded card, gray-200 border, shadow-sm |
| `.link` | sky-600 link, hover sky-700, no underline |
| `.btn` | base button (inline-flex, rounded-md, gap-2) |
| `.btn-primary` | sky-600 → hover sky-700, white text |
| `.btn-secondary` | white, gray-300 border, gray-700 text |
| `.btn-danger` | red-600 → hover red-700, white text |
| `.btn-sm` | compact button size |
| `.input` | full-width field, sky-500 focus ring |
| `.label` | form label (gray-700) |
| `.badge` | pill label (`rounded-full`, `text-xs`) |
| `.tbl` | fixed-layout table; truncating cells; row hover gray-50 |
| `.alert` + `.alert-{info,success,warning,danger}` | tonal callouts |
| `.doc` | long-form document typography (Rules, Format, Error pages) |

**Idiom:** style via Tailwind utility classes; reach for these component classes for the
repeated primitives. The palette is Tailwind's default **gray** neutral scale, with the one
brand accent defined as **`brand-*`** (a `@theme` alias of sky) so it lives in a single place.
Semantic tones (green / amber / red) stay on their Tailwind names.
