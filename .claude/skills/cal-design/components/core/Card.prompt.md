The standard CAL dark panel — surface-2 with a gradient keyline, used for every dashboard module. Optional `eyebrow` + `action` header.

```jsx
<Card eyebrow="Recent Seasons" action={<a href="/seasons">All →</a>}>
  <SeasonRow ... />
</Card>
```

Props: `eyebrow` (mono label), `action` (right-aligned header node), `keyline` (default true), `elevation` (`sm|md|lg`), `padded` (default true). Set `padded={false}` for edge-to-edge lists and pad rows individually.
