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

## Rating System — TrueSkill (Halo 3 Style)

- Every player has **μ (mu)** = estimated skill and **σ (sigma)** = uncertainty
- **Display rating** = μ − 3σ (conservative — you earn it)
- New players are **Unranked** for their first 5 games
- Sigma decreases as you play — your rating becomes more certain over time
- Beat a stronger player → larger gain. Lose to a weaker player → larger loss.
- All ratings are **public and visible** on the leaderboard. No hidden numbers.

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
