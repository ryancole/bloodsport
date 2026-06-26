---
name: cal-design
description: Use this skill to generate well-branded interfaces and assets for Champions Amateur League (CAL) — a competitive League of Legends amateur league platform — for production or throwaway prototypes/mocks. Contains CAL's design guidelines, colors, type, fonts, logo assets, design tokens, and reusable UI kit components for prototyping its dark neon esports dashboard look.
user-invocable: true
---

# Champions Amateur League (CAL) — Design Skill

**Where summoners become champions.** CAL is a dark, neon esports dashboard for running *League of Legends* amateur seasons and playoffs. The signature look: near-black navy surfaces, a **cyan→magenta** gradient keyline, condensed UPPERCASE Rajdhani headings, heavy Archivo display numerals, and JetBrains Mono labels.

## Start here

1. **Read `readme.md`** — the full design guide: brand context, content/voice rules, visual foundations, iconography, and a file-by-file index. This is your source of truth.
2. **Skim the tokens** in `tokens/` (`colors.css`, `typography.css`, `spacing.css`, `effects.css`) so you use the real CSS custom properties instead of inventing values.
3. **Browse the specimen cards** in `guidelines/` and the component demos in `components/*/` to see the system in use.

## How to build

- **Always link `styles.css`** (the single entry point — it `@import`s the webfonts and every token file), then style with the `--cal-*` custom properties. Never hard-code hexes that already exist as tokens.
- **Throwaway visuals (slides, mocks, prototypes):** copy the assets you need out of `assets/` and write self-contained static **HTML** files for the user to view. Reference `styles.css` with a relative path.
- **Production code:** read the rules here and reuse the token variables + component patterns to become an expert in designing with this brand. The reusable React primitives live in `components/` (`Button`, `Badge`, `Card`, `Eyebrow`, `StatCard`, `Input`, `TeamFlag`).
- **Stay on-brand:** dark mode only; cyan = primary/regular-season, magenta = playoffs; status = green/amber/red; badges are mono UPPERCASE ghost-fill pills; cards carry the 2px gradient keyline; motion is quick `cubic-bezier(0.22,1,0.36,1)` ease-out, no bounces. **No emoji** in UI copy. Voice is declarative and ruleset-precise.

## Assets & icons

- **Logo:** `assets/logo/CAL-masthead-*.png` (neon "CAL" glitch masthead) — present on a dark surface. `assets/favicon.svg` for the favicon.
- **Icons:** CAL has no custom icon set. Prefer typography/labels/badges; when an icon is truly needed, use thin-stroke single-color line icons (Lucide is the recommended match) and flag it as a substitution.

## If invoked with no specific task

Ask the user what they want to build or design, ask a few clarifying questions (audience, surface, fidelity, production vs. mock), then act as an expert CAL designer who outputs either HTML artifacts or production code depending on the need.
