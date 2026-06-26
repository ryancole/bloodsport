# CAL Web — UI Kit

A high-fidelity, interactive recreation of the **Champions Amateur League** web app (the Blazor product in `Websites/BloodsportSite/`). It composes the design system's React primitives — it does **not** re-implement them.

## Run it

Open `index.html`. It's a click-through prototype with fake (LoL-flavored) data — no backend.

## Screens

| Screen | File | What it shows |
|---|---|---|
| **Dashboard** | `Dashboard.jsx` | The flagship Home view — stat row, Recent Seasons / Recent Playoffs panels, Latest News, Recent Activity feed. |
| **Seasons** + **Season detail** | `Screens.jsx` | Season list; detail with **Team Standings** table (top-4 qualify) and a Week schedule. |
| **Teams** | `Screens.jsx` | Team cards with `TeamFlag` avatars and W/L badges. |
| **News** | `Screens.jsx` | Editorial post list in the CAL ruleset voice. |
| **Playoffs** | `Playoffs.jsx` | Single-elimination **bracket** — winners highlighted in cyan, lobby codes on active matchups. |
| **TopNav** | `TopNav.jsx` | Sticky dark nav with the neon masthead + Rajdhani uppercase links. |
| Format / Rules / Users | `index.html` | Light placeholders (not core to the recreation). |

`data.js` holds all mock data. `index.html` is the router that wires nav → screens.

## How it's built

- Loads the design system bundle (`../../_ds_bundle.js`) and reads components from `window.ChampionsAmateurLeagueDesignSystem_df3b47` (`Button`, `Badge`, `Card`, `StatCard`, `Eyebrow`, `Input`, `TeamFlag`).
- Links `../../styles.css` for tokens + webfonts.
- Each screen file assigns its component to `window` (Babel scripts don't share scope) — see the `Object.assign(window, …)` at the bottom of each `.jsx`.

## Fidelity notes

Visuals are lifted from the live `Home.razor` dashboard and the shared panel/table components (`DashboardSeasonsPanel`, `DashboardPlayoffsPanel`, `SeasonStandingsTable`, `PlayoffBracket`). The playoff bracket is rendered as styled HTML cards (the real app draws it on a `<canvas>`); everything else mirrors the product's dark dashboard layout. Cosmetic recreation only — no real routing, auth, or data.
