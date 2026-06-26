**Button** — primary action element; uppercase Archivo label on a dark UI. Use `primary` (cyan) for the main action per view, `gradient` for hero/marketing CTAs, `secondary` for adjacent actions, `ghost` for low-emphasis.

```jsx
<Button variant="primary" size="lg" onClick={join}>Join the league</Button>
<Button variant="secondary">View standings</Button>
<Button variant="gradient" iconRight={<span>→</span>}>Watch live</Button>
```

Variants: `primary` · `gradient` · `secondary` · `ghost`. Sizes: `sm` · `md` · `lg`. Hover brightens fills / lights outlines cyan; press nudges down 1px.
