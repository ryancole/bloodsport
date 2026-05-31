# BLOODSPORT
### League of Legends Tournament Platform

> *No mercy in the Rift.*

A self-hosted, competitive LoL tournament bracket system built by Stephen Crittenden and Ryan Cole. Inspired by the 1988 film and the Halo 3 TrueSkill ranking system.

---

## Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8 Web API |
| Frontend | Blazor WebAssembly (.NET 8) |
| Real-time | SignalR |
| Database | PostgreSQL + Entity Framework Core 8 |
| Auth | OpenIddict (self-hosted OpenID Connect) |
| Rating | TrueSkill (moserware/Skills) |
| Tournament | Riot Tournament API v5 |
| Deploy | Docker Compose |

---

## Quick Start (Ryan)

**Prerequisites:** .NET 8 SDK, Docker Desktop

```bash
git clone https://github.com/ryancole/bloodsport
cd bloodsport

# Start PostgreSQL
docker-compose up postgres -d

# Run the API
cd src/Bloodsport.Api
dotnet run

# Run the Blazor client (new terminal)
cd src/Bloodsport.Client
dotnet run
```

Open: http://localhost:5001

---

## Project Structure

```
bloodsport/
├── src/
│   ├── Bloodsport.Core/         # Domain models, TrueSkill, tournament logic, TDL parser
│   ├── Bloodsport.Data/         # EF Core DbContext, PostgreSQL migrations
│   ├── Bloodsport.Api/          # ASP.NET Core API, SignalR hub, Riot webhook
│   └── Bloodsport.Client/       # Blazor WebAssembly frontend
├── tournaments/
│   └── bloodsport_open.tournament.yml   # TDL — tournament configuration
└── docker-compose.yml
```

---

## Tournament Definition Language (TDL)

Tournaments are defined in YAML. Edit `tournaments/bloodsport_open.tournament.yml` to configure format, seeding, rating parameters, and Riot settings. No code changes needed.

```yaml
name: "Bloodsport Open"
format: single_elimination
players: 8
best_of: 3
seeding: trueskill
rating:
  algorithm: trueskill
  mu_initial: 25.0
  sigma_initial: 8.333
riot:
  region: NA1
  pick_type: TOURNAMENT_DRAFT
```

---

## Rating System — Bloodsport Rating (BSR)

The BSR is a six-component rating system designed to express true player skill more honestly than any binary win/loss system can. Every component is public and visible on the player profile. Nothing is hidden.

---

### Component 1 — TrueSkill Foundation

The mathematical backbone. Same Bayesian system that powered Halo 3's ranking. Every player has **μ (mu)** = estimated skill and **σ (sigma)** = uncertainty.

- **Display base** = μ − 3σ (conservative — you earn every visible point)
- Unranked for the first 5 games — the system needs data before it commits
- σ decreases as you play more — your rating becomes more certain over time
- Implemented via `moserware/Skills` NuGet — `BloodsportRatingService.cs`

---

### Component 2 — Performance Multiplier

Scales the magnitude of TrueSkill's rating update based on how well you actually played.

| Outcome | Multiplier |
|---------|-----------|
| Won and outperformed benchmark | 1.4× |
| Won at benchmark | 1.0× |
| Won but underperformed | 0.7× |
| Lost but outperformed | 0.7× (reduced penalty) |
| Lost at benchmark | 1.0× |
| Lost and underperformed | 1.4× |

Benchmarks are role-specific (KDA, CS/min, vision, damage/gold, objective participation). Winning badly gives you less than winning brilliantly. Losing while playing well costs you less than losing badly.

---

### Component 3 — Consistency Index

Measures how reliably you perform across your last 10 games. A player who delivers the same quality every game is more valuable than one who peaks high and crashes. Uses coefficient of variation on rolling performance scores.

- **Range:** −2.0 to +2.0 added directly to BSR
- Displayed as a 0–100 score on the player profile with a confidence bar
- Requires 3+ games to calculate — shown as neutral until then

---

### Component 4 — Build Intelligence Score (BIS)

Measures how efficiently and intelligently you build items. Three sub-scores:

| Sub-Score | Weight | What It Measures |
|-----------|--------|-----------------|
| Gold Efficiency | 40% | Did purchased items maximize stat value per gold spent? |
| Build Timing | 35% | Were core items completed at optimal windows? |
| Situational Adaptation | 25% | Did the build respond to the enemy composition and game state? |

- **Score:** 0–100. Contributes up to **+5 BSR points** for near-perfect building.
- A player who adapts their build mid-game to counter what's happening scores higher than one who blindly follows a tier list.
- Populated from Riot API match data. Mocked in beta.

---

### Component 5 — Expression Index

**The most unique component in competitive gaming rating. No other system has this.**

Measures individual creative expression — off-meta choices that win. Rewards players who have internalized the game deeply enough to break the meta intentionally and productively.

```
Expression Index = Meta Deviation × Outcome Correlation
```

**Meta Deviation** — how far your play diverges from population averages:
- Champion pick rate deviation (playing a 2% pick rate champion scores higher)
- Build divergence from the most common build for that champion
- Play pattern deviation (unique positioning, ability usage, macro decisions)

**Outcome Correlation** — does the deviation actually produce wins?
- High deviation + wins = authentic mastery. The system rewards it.
- High deviation + losses = creative but ineffective. Not rewarded.
- Low deviation + wins = skilled but conventional. TrueSkill handles it.

**The philosophy:** True mastery is not optimization. It is internalized knowledge expressed freely. Frank Dux did not follow the textbook in the Kumite final. He fought blind, from muscle memory, from something Tanaka had put in him that nobody could take away. The Expression Index measures that dimension of skill. A player who develops their own champion identity, their own creative approach to the game — and makes it work — is doing something more difficult than someone executing the meta efficiently.

- **Score:** 0–100. Contributes up to **+3 BSR points**.
- Generates a **Signature Style** label on the player profile (e.g. "Off-meta specialist", "Unconventional builder", "Pattern breaker")
- Requires 5+ games to calculate

---

### Component 6 — Honor Coefficient

Sportsmanship within Bloodsport tournaments. Post-match honors from tournament participants only — cannot be gamed by outsiders.

- 0.5% contribution per honor received, capped at **+2.0 BSR points**
- Visible on the player profile: honors received / games played
- Consistent with the Kumite code: you honor the opponent whether you win or lose

---

### The Full Formula

```
BSR = (TrueSkillBase × PerformanceMultiplier)
    + ConsistencyModifier
    + (BuildIntelligenceScore × 0.05)
    + (ExpressionIndex × 0.03)
    + HonorCoefficient
```

---

### Rank Tiers

| Tier | BSR | Description |
|------|-----|-------------|
| **Unranked** | < 5 games | Not yet tested |
| **Initiate** | 0–10 | You have entered the arena |
| **Warrior** | 10–18 | Proven in combat |
| **Veteran** | 18–26 | Tested across brackets |
| **Elite** | 26–34 | Few reach this. Fewer stay. |
| **Champion** | 34–42 | Tournament winners only |
| **Kumite** | 42+ | You did not choose this. It chose you. |

---

### Implementation

All rating logic lives in `src/Bloodsport.Core/Rating/`:

| File | Purpose |
|------|---------|
| `BloodsportRatingService.cs` | All six components, full BSR calculation, mock data generator |
| `BloodsportRating.cs` | The complete rating model — all components exposed publicly |
| `MatchPerformance.cs` | Per-match data model + role benchmarks |
| `TrueSkillService.cs` | Original TrueSkill wrapper (legacy — BSR service supersedes it) |

The mock data generator (`GenerateMockPerformance`) lets the system run fully in beta without real Riot API data. Wire `RiotMatchId` to real match data when the Tournament API key arrives — no structural changes needed.

---

## Riot API Setup

1. Apply for a production API key at https://developer.riotgames.com/
2. Request tournament API access (requires production key)
3. Add your key to `appsettings.json`:
   ```json
   "Riot": {
     "ApiKey": "RGAPI-your-key",
     "CallbackUrl": "https://your-domain.com/api/tournament/riot/callback"
   }
   ```
4. The callback URL must be publicly accessible — Riot POSTs match results to it

---

## Auth

Self-hosted OpenID Connect via **OpenIddict**. No Auth0, no Keycloak, no third parties.

- `POST /connect/token` — get a JWT (password flow for admin, PKCE for users)
- Roles: `Admin` (full control), `Organizer` (record results), `Player` (view only)

---

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/tournament` | List all tournaments |
| GET | `/api/tournament/{id}` | Get bracket + matches |
| POST | `/api/tournament` | Create tournament (Admin) |
| POST | `/api/tournament/{id}/start` | Seed bracket + generate Riot codes (Admin) |
| POST | `/api/tournament/{id}/matches/{matchId}/result` | Record match winner |
| POST | `/api/tournament/riot/callback` | Riot match result webhook |
| GET | `/api/players` | Leaderboard (ranked players) |
| GET | `/api/players/{id}` | Player profile |

---

## Deploy (Production)

```bash
cp .env.example .env
# Edit .env — add RIOT_API_KEY and RIOT_CALLBACK_URL

docker-compose up -d
```

All services run in Docker. PostgreSQL data is persisted in a named volume.

---

*Built with Claude Code — May 2026*
