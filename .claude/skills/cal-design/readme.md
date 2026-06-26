# CAL — Champions Amateur League · Design System

A dark, high-contrast brand built around one idea: the **RGB-split glitch**. White Archivo type lit by a cyan (`#00E5FF`) fringe on the left and a magenta (`#FF2E98`) fringe on the right, sitting on a cool near-black ground (`#0B0E14`). It started as a favicon mark — a glitched "C" tile — and this system extends that language into a full product surface for an open, competitive amateur sports/gaming league.

**Origin asset:** `assets/cal-masthead.png` (the original navbar logo) and `assets/cal-mark.png` / `assets/favicon.ico` (the derived app mark).

---

## Index / manifest

- **`styles.css`** — the single entry point consumers link. `@import`s the four token files.
- **`tokens/`** — `colors.css`, `typography.css`, `spacing.css`, `effects.css`.
- **`assets/`** — `cal-masthead.png` (wordmark), `cal-mark.png` (180px tile), `favicon.ico`, `favicon-32.png`.
- **`components/`** — reusable React primitives:
  - `core/` — **Button**, **Badge**, **Card**
  - `brand/` — **GlitchText** (the signature wordmark/headline treatment)
  - `data/` — **StatBlock**
  - `forms/` — **Input**
- **`ui_kits/website/`** — interactive CAL site: Home, Standings, Live match.
- **`guidelines/`** — foundation specimen cards (Colors, Type, Spacing, Brand).
- **`SKILL.md`** — Agent-Skill manifest for downloading into Claude Code.

---

## Content fundamentals

The voice is **terse, confident, broadcast-sport**. Short declaratives, present tense, occasional fragments used as punctuation: "Open brackets. Real stakes." Headlines speak directly to the competitor ("Where amateurs become champions.", "Enter your team"). It addresses **you**, never "users".

- **Casing:** Display headlines are sentence case but set in UPPERCASE via the glitch treatment. Labels, eyebrows, scores, metadata are **UPPERCASE mono with wide tracking** (`0.18em`–`0.32em`).
- **Numbers are heroes.** Scores, standings, viewer counts, prize pools are set large in Archivo Black or mono with tabular figures. Let data carry the page.
- **No emoji.** Status is shown with colored dots, badges, and mono labels — not emoji. (Unicode geometric marks like ⬤ ▰ are acceptable as compact event glyphs in dense data views.)
- **No filler.** Eyebrows are 2–4 words ("Season 04 · Now live"). Don't pad.

---

## Visual foundations

- **Ground:** cool near-black `#0B0E14`. Surfaces step up in lightness — `surface-1 #11151F` (wells/fields), `surface-2 #161B27` (cards), `surface-3 #1D2433` (hover). Never pure black, never warm.
- **Accents:** exactly two — **cyan `#00E5FF`** and **magenta `#FF2E98`** — plus the `105°` cyan→magenta **gradient** for hero CTAs and card keylines. Use accents as *signal*, not decoration; most of any screen is dark neutral.
- **The glitch** is the one signature move: `text-shadow: -Npx 0 cyan, Npx 0 magenta` on white text, where N ≈ 4.5% of font size. Wordmark, hero headlines, section heads only — **never on body or small text** (it muddies). Implemented as `GlitchText`.
- **Type:** **Archivo** (400–900) for everything structural; **JetBrains Mono** (400–700) for labels, data, eyebrows. Display tracking is tight (`-0.01em`); mono tracking is wide.
- **Cards:** dark slabs — `surface-2`, 1px `--cal-border` hairline, `--cal-radius-lg` (18px) corners, a deep ambient shadow (`0 16px 40px rgba(0,0,0,.45)`) and an `inset 0 1px 0 rgba(255,255,255,.04)` top edge for a crisp lit lip. Optional 2px gradient/cyan/magenta **keyline** across the top.
- **Borders:** hairlines do the dividing work (`--cal-border` / `--cal-border-faint`), not heavy rules. Tables separate rows with `border-faint` only.
- **Corners:** 6 → 24px scale; the **icon tile uses `22%`** rounding (the favicon ratio).
- **Shadows:** ambient and dark only (no colored drop shadows). Reserve **glows** (`--cal-glow-*`) for genuinely active/live elements.
- **Transparency & blur:** the sticky nav uses `rgba(11,14,20,.82)` + `backdrop-filter: blur(12px)`. Ghost tints (`--cal-*-ghost`, 12%) back badges.
- **Motion:** quick and confident. `--cal-ease` (`cubic-bezier(.22,1,.36,1)`), 120–360ms. Buttons brighten on hover and nudge down 1px on press; interactive cards lift `-2px`. The only looping animation is the **LIVE pulse** (expanding magenta ring, 1.6s).
- **Hover/press:** fills brighten (`filter: brightness(1.08)`); outlines and ghost text light **cyan**; press = `translateY(1px)`.
- **Focus:** cyan border + soft cyan glow ring (`0 0 0 3px var(--cal-cyan-ghost)`).
- **Imagery:** photography (when used) should be cool-toned, high-contrast, slightly desaturated to sit in the dark world. No warm/sepia grades. This system ships no stock imagery — drop real photos into card/hero slots.

---

## Iconography

CAL has **no custom icon font**. Approach:
- **Status & events** are communicated with colored **dots, badges, and mono labels** rather than pictographic icons (see `Badge`, the LIVE pulse, the match timeline).
- Team crests are **monogram tiles** (2-letter, Archivo Black on a solid accent), not logos — a deliberate amateur-league convention.
- Where line icons are genuinely needed (nav chevrons, arrows), use a **single inline glyph** (`→`) or pull **Lucide** from CDN (`https://unpkg.com/lucide-static`) at a 1.75px stroke to match the type weight. **Flag any Lucide use** — it's a substitution, not a bespoke set.
- **No emoji** anywhere in product UI.

---

## Fonts — substitution note

Both families are **Google Fonts**, imported at the top of `tokens/typography.css`:
- **Archivo** — display + body. (The original masthead is a bold condensed grotesque; Archivo is the closest free match and the working brand face.)
- **JetBrains Mono** — labels/data.

No binary font files are vendored — consumers pull them from Google Fonts at runtime. If you need offline/self-hosted webfonts, ask and I'll vendor the `woff2` files and rewrite the `@font-face` rules.
