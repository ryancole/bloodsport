Uppercase Rajdhani action button — the default CAL call-to-action; solid-cyan `primary` for commit actions, `neon` (cyan ghost) for in-flow links, `secondary`/`ghost` for low emphasis.

```jsx
<Button variant="primary" size="md" onClick={register}>Register Team</Button>
<Button variant="neon">View Bracket</Button>
<Button variant="secondary" size="sm">Cancel</Button>
```

Variants: `primary` (solid cyan, dark text), `neon` (cyan ghost), `secondary` (outlined surface), `ghost` (text only), `danger`. Sizes `sm | md | lg`. Supports `iconLeft` / `iconRight`, `disabled`. Press nudges down 1px; hover brightens.
