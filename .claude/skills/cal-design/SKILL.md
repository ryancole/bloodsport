---
name: cal-design
description: Use this skill to generate well-branded interfaces and assets for CAL (Champions Amateur League), either for production or throwaway prototypes/mocks/etc. Contains essential design guidelines, colors, type, fonts, assets, and UI kit components for prototyping. The look is a dark cool-black ground with a cyan/magenta RGB-split "glitch" wordmark.
user-invocable: true
---

Read the `readme.md` file within this skill, and explore the other available files (`styles.css` + `tokens/`, `components/`, `ui_kits/`, `guidelines/`, `assets/`).

If creating visual artifacts (slides, mocks, throwaway prototypes, etc), copy assets out and create static HTML files for the user to view, linking `styles.css` for the real tokens. If working on production code, copy assets and read the rules here to become an expert in designing with this brand.

Core rules to honor:
- Cool near-black ground (`#0B0E14`); two accents only — cyan `#00E5FF` + magenta `#FF2E98` — plus their gradient.
- Archivo for structure, JetBrains Mono (wide uppercase tracking) for labels/data.
- The "glitch" (`text-shadow: -N 0 cyan, N 0 magenta` on white) is for the wordmark and hero headlines only — never body or small text.
- Dark slab cards, hairline borders, ambient shadows, no emoji, numbers as heroes.

If the user invokes this skill without other guidance, ask them what they want to build, ask a few questions, and act as an expert designer who outputs HTML artifacts _or_ production code, depending on the need.
