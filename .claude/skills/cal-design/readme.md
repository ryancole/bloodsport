# Champions Amateur League — Design System

> **Where summoners become champions.**

Champions Amateur League (**CAL**) is a community-run, competitive *League of Legends* amateur league. The platform lets organizers run **seasons** (round-robin regular play) and single-elimination **playoffs**, while players form **teams**, link their **Riot accounts**, and track standings, brackets, and match results.

This design system captures CAL's visual + verbal identity: a **dark esports dashboard** built on a neon **cyan→magenta** signature, condensed display type, and a precise, ruleset-style voice.

---

## Sources

This system was reverse-engineered from the live product. If you have access, explore these to go deeper:

- **Codebase (attached):** `Websites/BloodsportSite/` — an ASP.NET Core / **Blazor** (.NET 10) app. The flagship `Components/Pages/Home.razor` dashboard and `wwwroot/app.css` are the source of truth for the dark token system; layout lives in `Components/Layout/`.
- **GitHub:** [`ryancole/bloodsport`](https://github.com/ryancole/bloodsport) — the same application (domain models, EF data layer, Blazor site). The repo README covers the stack and Azure/Entra deployment. Browse it to understand data shapes (Season, Team, Playoff, RiotAccount) behind the UI.

> Note: the product began life as "Bloodsport / BSAL" and was rebranded to **Champions Amateur League (CAL)**. Some legacy page titles still read "BSAL" — treat **CAL / Champions Amateur League** as canonical.

---

## Content fundamentals

How CAL writes.

- **Two registers, one product.** Marketing surfaces (tagline, hero) are **aspirational and terse**; functional surfaces (rules, format, dashboards) are **declarative and exact**.
- **Tagline voice:** *"Where summoners become champions."* — short, second-nature *LoL* vocabulary ("summoners"), no hype words, ends on the brand noun.
- **Rules/format voice:** numbered clauses (`1.10`, `2.20`), third person, present tense, no hedging. e.g. *"Playoff matchups are best-of-one. One loss and a team is eliminated from the bracket."* Declarative sentences state the rule, then a plain-language consequence.
- **Casing:** UI labels, nav, badges and eyebrows are **UPPERCASE** with wide tracking (the mono/Rajdhani treatment). Body copy and titles are sentence case.
- **Person:** product speaks *about* teams and players in the **third person** ("teams are seeded…"), not "you". Reserve "you" for direct CTAs.
- **Data is terse and abbreviated:** `W 5 — L 1`, `Best of One`, `Reg. Open`, `Active`, `4F2A-9KQ7`. Mono, uppercase, no full sentences.
- **No emoji.** None in product copy. The one decorative exception in the live app is a 🏆 on the Grand Final banner — used sparingly as an event flourish, never in running text.
- **Vibe:** competitive, official, a little bit *arcade*. Think tournament broadcast lower-thirds, not a casual social app.

---

## Visual foundations

The complete answer to "what does CAL look like?"

- **Surface & mood.** Near-black navy canvas (`--cal-bg #0B0E14`) with a tight ladder of raised surfaces (`surface-1/2/3`). Everything is **dark mode only**. Cards float on deep ambient shadow plus a 1px inset top highlight (`--cal-edge-top`) that simulates light catching the top edge.
- **Color.** Two neon brand hues — **cyan `#00E5FF`** (regular season / primary / links / focus) and **magenta `#FF2E98`** (playoffs / secondary). The **cyan→magenta 105° gradient** (`--cal-gradient`) is the signature, used as big stat numerals (background-clipped text) and as a **2px keyline** across the top of every card. Status colors: green `#21D07A` (win / registration open), amber `#FFB020` (preseason / pending), red `#FF4D4D` (loss / eliminated). *(An older championship-gold accent was retired with its legacy wordmark logos — the system is now neon-forward.)*
- **Type.** Three families do the work: **Archivo** (body + heavy 900 display numerals, tracking `-0.02em`), **Rajdhani** (condensed UPPERCASE headings and nav, tracking `0.04em`), **JetBrains Mono** (eyebrows, labels, data, codes — UPPERCASE, tracking `0.12–0.24em`). Exo 2 is an occasional sci-fi accent. Headings are tight and condensed; labels are wide and monospaced — the contrast *is* the identity.
- **Spacing & layout.** Compact dashboard rhythm on a ~14px signature gutter (`--cal-space-6`). Main content column maxes at `1200px`; a `3.5rem` sticky top nav. Dense grids of stat tiles, panels, and list rows.
- **Radii.** Soft but not pill-y for containers: `xs 6 / sm 10 / md 14 / lg 18` (cards default to **18px**); full pills only for badges.
- **Cards.** Surface-2 fill, 1px `--cal-border` hairline, `radius-lg`, shadow (`sm/md/lg`) + `--cal-edge-top`, and the gradient keyline on top. An optional header pairs a mono **eyebrow** (left) with an action link (right).
- **Badges.** Pill, mono, UPPERCASE, **ghost-fill** (12% alpha of the hue) with the saturated hue as text — never solid fills. They map to league state.
- **Borders & dividers.** Internal list rows separate with the faintest border (`--cal-border-faint`); emphasized edges use `--cal-border-strong`.
- **Motion.** Subtle and quick. Signature ease-out `cubic-bezier(0.22, 1, 0.36, 1)`, `200ms` default (`120ms` fast). Color/background transitions on hover; **no** bounces, no infinite decorative loops.
- **Hover states.** Links and list items shift toward cyan; rows lift to `surface-3`; buttons brighten ~12%. **Press:** buttons nudge down 1px (`translateY(1px)`).
- **Focus.** Neon ring — `0 0 0 2px var(--cal-bg), 0 0 0 4px var(--cal-cyan)`; inputs lift their border to cyan with a soft cyan ghost halo.
- **Glow.** Reserved, used sparingly on neon emphasis (`--cal-glow-cyan/magenta`) — e.g. a logo hover or a live indicator, not on every element.
- **Imagery.** Largely UI-driven (no photography in the core product). Team/player identity is expressed through small square **TeamFlag** avatars (logo image, or a gradient tile with initials). Backgrounds stay flat dark — texture comes from the gradient keylines and neon, not patterns.

---

## Iconography

- **The brand has no custom icon font or large SVG icon set.** The live product is mostly typographic; what few glyphs exist are **inline single-purpose SVGs** (e.g. the Reddit and email marks in the footer, drawn as small `viewBox="0 0 20 20"` circles).
- **Approach for new work:** prefer **typography, mono labels, and badges** over decorative icons. When an icon is genuinely needed, use a **thin-stroke, single-color** line icon that reads at small sizes on dark — [Lucide](https://lucide.dev) (CDN: `https://unpkg.com/lucide-static`) is the closest match to the product's clean, geometric feel and is the recommended substitute. **Flag any icon set as a substitution** — there is no official CAL icon library to match against.
- **No emoji as UI** (the lone 🏆 Grand Final flourish aside). Avoid unicode-glyph icons.
- **Logo:** the primary mark is the **CAL neon masthead** (`assets/logo/CAL-masthead-*.png`) — a chromatic-aberration "CAL" glitch over "CHAMPIONS AMATEUR LEAGUE", shown on a `#2a2a2a` bar in the nav. `assets/favicon.svg` is the offset-"CAL" favicon. Always present the masthead on a dark surface.

---

## What's in here (index)

**Foundations (root + `tokens/`)**
- `styles.css` — the single entry point consumers link. `@import`s the webfonts + every token file. *Imports nothing inline.*
- `tokens/colors.css` — surfaces, borders, text, neon brand, semantic status, gradients + semantic aliases.
- `tokens/typography.css` — font families, weights, type scale, tracking.
- `tokens/spacing.css` — spacing scale, radii, layout maxima.
- `tokens/effects.css` — shadows, inset edge, glows, focus ring, motion (ease/duration).
- `tokens/base.css` — element resets / dark defaults.

**Specimen cards (`guidelines/`)** — populate the Design System tab. Groups: **Colors** (surfaces, brand, semantic, gradient, text), **Type** (display, headings, mono, scale), **Spacing** (scale, radii, elevation), **Brand** (logo, voice).

**Components (`components/`)** — namespace `window.ChampionsAmateurLeagueDesignSystem_df3b47`
- `core/` — `Button`, `Badge`, `Card`, `Eyebrow`, `StatCard`
- `forms/` — `Input`
- `data/` — `TeamFlag`

**Assets (`assets/`)** — `logo/` (CAL neon masthead at multiple sizes), `favicon.svg` / `favicon.png`.

---

## Status / roadmap

Built: tokens, foundation cards, and the core component set. **Not yet built:** a full product **UI kit** (dashboard / season / playoff screens) and `SKILL.md`. Pick these up next to complete the system.
