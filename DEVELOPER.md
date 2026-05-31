# BLOODSPORT — Developer Reference
*For Ryan Cole — everything you need to complete the project*

---

## State of the Codebase

The scaffold is complete. Four projects, all wired together, all compiling cleanly once you restore packages. What you have is a working skeleton with the critical domain logic already implemented. What you do not have yet is the auth flow, a few missing API endpoints, and the Blazor pages for player registration and admin management.

Read this document top to bottom once. Then work the checklist at the bottom.

---

## Project Map

```
bloodsport/
├── src/
│   ├── Bloodsport.Core/              # No dependencies on other projects
│   │   ├── Models/
│   │   │   ├── Player.cs             # Domain model — TrueSkill properties, DisplayRating computed
│   │   │   └── Tournament.cs         # Tournament, Match, TournamentPlayer, enums
│   │   ├── Rating/
│   │   │   └── TrueSkillService.cs   # Wraps moserware/Skills — CalculateMatchOutcome, WinProbability
│   │   ├── Tournament/
│   │   │   └── BracketService.cs     # GenerateBracket, AdvanceBracket — standard seed ordering
│   │   └── Tdl/
│   │       └── TournamentDefinition.cs  # YAML parser for *.tournament.yml files
│   │
│   ├── Bloodsport.Data/              # Depends on Core
│   │   └── BloodsportDbContext.cs    # EF Core — all tables, OpenIddict tables via UseOpenIddict()
│   │
│   ├── Bloodsport.Api/               # Depends on Core + Data
│   │   ├── Program.cs                # Full startup — OpenIddict configured, SignalR, CORS
│   │   ├── Controllers/
│   │   │   ├── TournamentController.cs
│   │   │   └── PlayersController.cs
│   │   ├── Hubs/
│   │   │   └── BracketHub.cs         # SignalR — JoinTournament / LeaveTournament
│   │   └── Services/
│   │       └── RiotTournamentService.cs
│   │
│   └── Bloodsport.Client/            # Blazor WASM — depends on Core only
│       ├── Pages/
│       │   ├── Index.razor           # Live bracket — SignalR connected
│       │   └── Leaderboard.razor     # TrueSkill rankings
│       ├── Shared/
│       │   └── MainLayout.razor
│       └── wwwroot/
│           ├── index.html
│           └── css/bloodsport.css    # Full Bane/Mesrine aesthetic
│
├── tournaments/
│   └── bloodsport_open.tournament.yml   # TDL config — edit this, no code changes needed
├── docker-compose.yml
├── MARKETING.md
└── DEVELOPER.md                         # This file
```

---

## Data Flow — How Everything Connects

```
Player registers (POST /api/players)
    └─► Player row created, TrueSkillMu=25, TrueSkillSigma=8.333

Admin creates tournament (POST /api/tournament)
    └─► Tournament row created, Status=Draft, DefinitionYaml stored

Admin adds players to tournament
    └─► TournamentPlayer rows created with seed ratings snapshotted

Admin starts tournament (POST /api/tournament/{id}/start)
    └─► BracketService.GenerateBracket() — seeds players by DisplayRating
    └─► Riot API: CreateTournament() → tournamentId
    └─► Riot API: GenerateTournamentCodesAsync() → codes for Round 1 matches
    └─► Match rows created with RiotTournamentCodes assigned
    └─► SignalR: BracketUpdated broadcast to all connected clients

Players play the match in LoL client using the lobby code
    └─► Riot POSTs result to POST /api/tournament/riot/callback
    └─► Match.RiotMatchId stored

Admin records winner (POST /api/tournament/{id}/matches/{matchId}/result)
    └─► TrueSkillService.CalculateMatchOutcome() — updates both players' μ and σ
    └─► BracketService.AdvanceBracket() — moves winner to next match slot
    └─► If next match is now fully populated: Riot generates new lobby code
    └─► If this was the final match: Tournament.Status = Completed
    └─► SignalR: BracketUpdated broadcast → Index.razor re-renders live
```

---

## What Still Needs To Be Built

### 1. Fix: Register Riot HttpClient in Program.cs (30 minutes)

`RiotTournamentService` takes `IHttpClientFactory` but it is not registered. Add this to `Program.cs` before `var app = builder.Build()`:

```csharp
builder.Services.AddHttpClient("riot", client =>
{
    client.BaseAddress = new Uri("https://americas.api.riotgames.com/");
    client.DefaultRequestHeaders.Add("X-Riot-Token", builder.Configuration["Riot:ApiKey"]);
});
```

---

### 2. Build: AuthController (2–4 hours)

OpenIddict is fully configured in `Program.cs`. It handles token issuance, validation, and refresh automatically. You just need the controller actions that OpenIddict will route through.

Create `src/Bloodsport.Api/Controllers/AuthController.cs`:

```csharp
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Bloodsport.Data;
using System.Security.Claims;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm)
    {
        _userManager = um;
        _signInManager = sm;
    }

    // OpenIddict routes this from /connect/token
    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request not found.");

        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username!);
            if (user == null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
            identity.AddClaim(OpenIddictConstants.Claims.Email, user.Email!);
            identity.AddClaim(OpenIddictConstants.Claims.Name, user.UserName!);

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                identity.AddClaim(OpenIddictConstants.Claims.Role, role);

            identity.SetScopes(OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.Email);
            identity.SetDestinations(_ => new[] {
                OpenIddictConstants.Destinations.AccessToken,
                OpenIddictConstants.Destinations.IdentityToken
            });

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest("Unsupported grant type.");
    }

    // User registration
    [HttpPost("api/auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Username, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "Player");
        return Ok(new { user.Id, user.UserName, user.Email });
    }
}

public record RegisterRequest(string Username, string Email, string Password);
```

---

### 3. Build: DbInitializer — Seed Admin User (1 hour)

Create `src/Bloodsport.Api/DbInitializer.cs`:

```csharp
using Bloodsport.Data;
using Microsoft.AspNetCore.Identity;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var config = services.GetRequiredService<IConfiguration>();

        // Create roles
        foreach (var role in new[] { "Admin", "Organizer", "Player" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // Seed admin from config (appsettings.Development.json — gitignored)
        var adminEmail = config["Seed:AdminEmail"];
        var adminPassword = config["Seed:AdminPassword"];
        if (adminEmail == null || adminPassword == null) return;

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser { UserName = "admin", Email = adminEmail };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
```

Add to `Program.cs` inside the `if (app.Environment.IsDevelopment())` block:

```csharp
using var scope = app.Services.CreateScope();
await DbInitializer.SeedAsync(scope.ServiceProvider);
```

Add `appsettings.Development.json` (already gitignored):

```json
{
  "Seed": {
    "AdminEmail": "admin@bloodsport.local",
    "AdminPassword": "Bloodsport#2026"
  }
}
```

---

### 4. Build: GET /api/tournament/active (20 minutes)

The Blazor `Index.razor` calls this on load. Add to `TournamentController.cs`:

```csharp
[HttpGet("active")]
public async Task<IActionResult> GetActive()
{
    var tournament = await _db.Tournaments
        .Include(t => t.Players).ThenInclude(tp => tp.Player)
        .Include(t => t.Matches).ThenInclude(m => m.Player1)
        .Include(t => t.Matches).ThenInclude(m => m.Player2)
        .Include(t => t.Matches).ThenInclude(m => m.Winner)
        .Where(t => t.Status == TournamentStatus.Active || t.Status == TournamentStatus.Draft)
        .OrderByDescending(t => t.CreatedAt)
        .FirstOrDefaultAsync();

    if (tournament == null) return NotFound();
    return Ok(tournament);
}
```

---

### 5. Build: Add Players to Tournament Endpoint (30 minutes)

The admin needs to add registered players to a tournament before starting it. Add to `TournamentController.cs`:

```csharp
[HttpPost("{id}/players")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AddPlayer(Guid id, [FromBody] AddPlayerRequest request)
{
    var tournament = await _db.Tournaments.FindAsync(id);
    var player = await _db.Players.FindAsync(request.PlayerId);
    if (tournament == null || player == null) return NotFound();

    var tp = new TournamentPlayer
    {
        Id = Guid.NewGuid(),
        TournamentId = id,
        PlayerId = request.PlayerId,
        MuAtRegistration = player.TrueSkillMu,
        SigmaAtRegistration = player.TrueSkillSigma
    };

    _db.TournamentPlayers.Add(tp);
    await _db.SaveChangesAsync();
    return Ok(tp);
}

public record AddPlayerRequest(Guid PlayerId);
```

---

### 6. Build: Blazor Pages (4–6 hours total)

Three pages needed. Patterns already established in `Index.razor` and `Leaderboard.razor` — follow the same `@inject HttpClient Http` and `@code { }` structure.

**`Pages/Register.razor`** — Player registration form
- Route: `@page "/register"`
- Form fields: Username, Email, Summoner Name, Riot PUUID, Password
- Calls: `POST /api/auth/register` then `POST /api/players`
- On success: redirect to `/`

**`Pages/Login.razor`** — Login form
- Route: `@page "/login"`
- Form fields: Username, Password
- Calls: `POST /connect/token` with `grant_type=password`
- Store JWT in `localStorage` via JS interop
- On success: redirect to `/`

**`Pages/Admin.razor`** — Bracket management (auth guard)
- Route: `@page "/admin"`
- Requires Admin role — redirect to `/login` if not authenticated
- Sections:
  - Create tournament (textarea for YAML, calls `POST /api/tournament`)
  - Add players to draft tournament (player list, calls `POST /api/tournament/{id}/players`)
  - Start tournament button (calls `POST /api/tournament/{id}/start`)
  - Record match results (match list with winner dropdown, calls `POST /api/tournament/{id}/matches/{matchId}/result`)

---

### 7. Build: Entry Fee / Payment (Stripe — 3–4 hours)

The tournament charges $5 entry. The cleanest approach at this stage is Stripe Checkout — same pattern used on crittendencompany.com.

Add to `Bloodsport.Api.csproj`:
```xml
<PackageReference Include="Stripe.net" Version="45.0.0" />
```

Add to `Program.cs`:
```csharp
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
```

Create `Controllers/PaymentController.cs`:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly BloodsportDbContext _db;

    public PaymentController(IConfiguration config, BloodsportDbContext db)
    {
        _config = config;
        _db = db;
    }

    // Creates a Stripe Checkout session for tournament entry
    [HttpPost("entry/{tournamentId}")]
    public async Task<IActionResult> CreateEntrySession(Guid tournamentId)
    {
        var tournament = await _db.Tournaments.FindAsync(tournamentId);
        if (tournament == null) return NotFound();

        var options = new Stripe.Checkout.SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        UnitAmount = 500, // $5.00 in cents
                        Currency = "usd",
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Bloodsport — {tournament.Name} Entry",
                            Description = "Tournament entry. No smurfs. True honor."
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = $"{_config["ClientUrl"]}/entry-confirmed?tournamentId={tournamentId}",
            CancelUrl = $"{_config["ClientUrl"]}/"
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);
        return Ok(new { url = session.Url });
    }

    // Stripe webhooks confirm payment — then register player to tournament
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeEvent = Stripe.EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            _config["Stripe:WebhookSecret"]);

        if (stripeEvent.Type == Stripe.Events.CheckoutSessionCompleted)
        {
            // TODO: parse metadata, register player to tournament
            // Add PlayerId and TournamentId to session metadata in CreateEntrySession
        }

        return Ok();
    }
}
```

Add to `appsettings.json`:
```json
"Stripe": {
  "SecretKey": "sk_live_YOUR_KEY",
  "WebhookSecret": "whsec_YOUR_SECRET"
}
```

---

## Known Issues / Traps

| Issue | Location | Fix |
|-------|----------|-----|
| `IHttpClientFactory` not registered | `Program.cs` | Add `AddHttpClient("riot", ...)` — see item 1 above |
| EF migration not created | `Bloodsport.Data` | Run `dotnet ef migrations add Initial --startup-project ../Bloodsport.Api` |
| OpenIddict needs `UseOpenIddict()` in DbContext | `BloodsportDbContext.cs` | Already called in `OnModelCreating` — do not remove it |
| Blazor WASM can't use `[Authorize]` directly | Client pages | Use `AuthenticationState` + redirect pattern — see Blazor WASM auth docs |
| SignalR `BracketUpdated` event sends full tournament graph | `TournamentController.cs` | This will cause circular reference serialization errors with EF navigation properties — add `[JsonIgnore]` to back-navigation properties or use DTOs before this becomes a problem |
| `Champion` property on `Tournament` uses LINQ — not EF queryable | `Tournament.cs` | Computed in-memory only after data is loaded — do not try to use it in a `.Where()` clause |

---

## EF Core Migration — Run These Commands In Order

```bash
# From repo root
cd src/Bloodsport.Data

dotnet ef migrations add Initial \
  --startup-project ../Bloodsport.Api \
  --output-dir Migrations

dotnet ef database update \
  --startup-project ../Bloodsport.Api
```

If you add a model property later:
```bash
dotnet ef migrations add [DescriptiveName] --startup-project ../Bloodsport.Api
dotnet ef database update --startup-project ../Bloodsport.Api
```

---

## API Reference — All Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/connect/token` | None | Get JWT — `grant_type=password`, `username`, `password` |
| POST | `/api/auth/register` | None | Register new player account |
| GET | `/api/tournament` | None | List all tournaments |
| GET | `/api/tournament/active` | None | Most recent active or draft tournament |
| GET | `/api/tournament/{id}` | None | Full tournament with bracket |
| POST | `/api/tournament` | Admin | Create tournament from TDL YAML |
| POST | `/api/tournament/{id}/players` | Admin | Add player to tournament |
| POST | `/api/tournament/{id}/start` | Admin | Seed bracket + generate Riot codes |
| POST | `/api/tournament/{id}/matches/{matchId}/result` | Admin/Organizer | Record match winner |
| POST | `/api/tournament/riot/callback` | None (Riot signs it) | Riot match result webhook |
| GET | `/api/players` | None | Ranked leaderboard |
| GET | `/api/players/{id}` | None | Player profile |
| POST | `/api/players` | Admin | Create player |
| POST | `/api/payment/entry/{tournamentId}` | Player | Create Stripe Checkout session |
| POST | `/api/payment/webhook` | None (Stripe signs it) | Stripe payment confirmation |

---

## Fund Allocation — What Gets Built for the $5 Entry Fee

Every tournament entry is $5. The breakdown is public and published after every bracket closes.

| Allocation | % | Purpose |
|------------|---|---------|
| Prize pool | 35% | Top finishers — 25% / 7% / 3% split |
| Community events | 25% | LAN parties, local meetups, watch parties |
| Platform infrastructure | 20% | Hosting, database, Riot API, development |
| Grassroots tournament fund | 10% | Future brackets + community tournaments under the same honor code |
| Coaching & education | 10% | Accessible coaching resources |

The `PaymentController` webhook handler is where you track which allocation bucket each entry contributes to. Simplest implementation: a `FundLedger` table with a row per tournament close-out, storing the five amounts. Admin page shows the running totals.

---

## TrueSkill — How It Works in This Codebase

Every player starts at μ=25, σ=8.333.

Display rating = μ − 3σ (conservative estimate — you earn your visible number).

After each match:
```csharp
var (winnerUpdate, loserUpdate) = _trueSkillService.CalculateMatchOutcome(winner, loser);
winner.TrueSkillMu = winnerUpdate.NewMu;
winner.TrueSkillSigma = winnerUpdate.NewSigma;
// same for loser
```

σ decreases with each game played — the system becomes more certain about your real skill level. A player who wins 10 games and a player who wins 1 game can have the same μ but very different σ, which means very different display ratings.

Players are shown as "Unranked" in the UI until `GamesPlayed >= 5`. The `IsRanked` property on `Player` handles this.

---

## Checklist — In Order

- [ ] `dotnet restore`
- [ ] Register Riot HttpClient in `Program.cs`
- [ ] Run EF migration + database update
- [ ] Seed admin user (`DbInitializer`)
- [ ] Build `AuthController` — password flow + registration
- [ ] Add `GET /api/tournament/active`
- [ ] Add `POST /api/tournament/{id}/players`
- [ ] Build `Register.razor`, `Login.razor`, `Admin.razor`
- [ ] End-to-end test: register player → create tournament → add players → start → record result → verify bracket advances + ratings update + SignalR fires
- [ ] Add Stripe payment flow
- [ ] Add `FundLedger` table + admin totals view
- [ ] Legal check: Texas paid contest law before accepting first $5

---

*Questions → open a GitHub issue. Code review → tag @scritter007.*
