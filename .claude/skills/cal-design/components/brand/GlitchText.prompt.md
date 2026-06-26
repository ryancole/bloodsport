**GlitchText** — the brand's defining move: white Archivo Black with a cyan fringe left and magenta fringe right, mirroring the favicon mark. Reserve it for the wordmark, hero headlines, section heads and hero numbers. Never use on body copy or anything below ~24px (the fringe muddies small text).

```jsx
<GlitchText as="h1" size={76}>CAL</GlitchText>
<GlitchText as="h2" size={38}>Champions Amateur League</GlitchText>
```

Tune `split` for fringe distance and `intensity` for fringe opacity. On busy backgrounds, drop `intensity` to ~0.7.
