using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.ServiceBus;
using Bloodsport.Entity.Database;
using BloodsportSite.Services;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class SeasonEndpoints
    {
        public static IEndpointRouteBuilder MapSeasons(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/seasons/create", CreateAsync)
                .RequireAuthorization();

            endpoints.MapPost("/seasons/{id}/edit", EditAsync)
                .RequireAuthorization();

            endpoints.MapPost("/seasons/{id}/register", RegisterAsync)
                .RequireAuthorization();

            endpoints.MapPost("/seasons/{id}/unregister", UnregisterAsync)
                .RequireAuthorization();

            endpoints.MapPost("/seasons/{id}/build", BuildAsync)
                .RequireAuthorization();


            return endpoints;
        }

        // Admin: create a season
        private static async Task<IResult> CreateAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            RiotTournamentClient riotClient,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] string name,
            [Microsoft.AspNetCore.Mvc.FromForm] DateTime startDate,
            [Microsoft.AspNetCore.Mvc.FromForm] DateTime endDate)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            name = name.Trim();

            if (string.IsNullOrEmpty(name))
                return Results.Redirect("/seasons/create?error=invalid_name");

            if (endDate <= startDate)
                return Results.Redirect("/seasons/create?error=invalid_dates");

            await using var db = dbFactory.CreateDbContext();

            var season = new Season
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
            };

            db.Seasons.Add(season);
            await db.SaveChangesAsync();

            // Provision a Riot provider and tournament for this season.
            // Failures are logged but don't block season creation — IDs can be set manually if needed.
            try
            {
                var callbackUrl = configuration["RiotApi:CallbackUrl"]
                    ?? throw new InvalidOperationException("RiotApi:CallbackUrl is not configured.");

                season.RiotProviderId = await riotClient.CreateProviderAsync(callbackUrl);
                season.RiotTournamentId = await riotClient.CreateTournamentAsync(season.RiotProviderId.Value, name);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger(nameof(SeasonEndpoints));
                logger.LogError(ex, "Failed to provision Riot tournament for season {seasonId}. Tournament codes will be unavailable until IDs are set.", season.Id);
            }

            return Results.Redirect("/seasons");
        }

        // Admin: edit a season
        private static async Task<IResult> EditAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] SeasonStatus status,
            [Microsoft.AspNetCore.Mvc.FromForm] bool? registrationOpen = null)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            await using var db = dbFactory.CreateDbContext();
            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.Redirect("/seasons?error=season_not_found");

            season.Status = status;
            season.RegistrationOpen = registrationOpen ?? false;

            await db.SaveChangesAsync();

            return Results.Redirect($"/seasons/{id}");
        }

        // Captain: register their team for a season
        private static async Task<IResult> RegisterAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] long teamId)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect($"/seasons/{id}?error=not_authenticated");

            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.Redirect("/seasons?error=season_not_found");

            if (!season.RegistrationOpen)
                return Results.Redirect($"/seasons/{id}?error=registration_closed");

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.ManagerId == user.Id);

            if (team is null)
                return Results.Redirect($"/seasons/{id}?error=team_not_found");

            var alreadyRegistered = await db.SeasonRegistrations
                .AnyAsync(r => r.SeasonId == id && r.TeamId == teamId);

            if (alreadyRegistered)
                return Results.Redirect($"/seasons/{id}?error=already_registered");

            db.SeasonRegistrations.Add(new SeasonRegistration
            {
                SeasonId = id,
                Season = season,
                TeamId = teamId,
                Team = team,
            });

            await db.SaveChangesAsync();

            return Results.Redirect($"/seasons/{id}");
        }

        // Captain: remove their team's registration from a season while registration is still open
        private static async Task<IResult> UnregisterAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] long teamId)
        {
            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Redirect($"/seasons/{id}?error=not_authenticated");

            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.Redirect($"/teams/{teamId}?error=season_not_found");

            if (!season.RegistrationOpen)
                return Results.Redirect($"/teams/{teamId}?error=registration_closed");

            var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.ManagerId == user.Id);

            if (team is null)
                return Results.Redirect($"/teams/{teamId}?error=team_not_found");

            var registration = await db.SeasonRegistrations
                .FirstOrDefaultAsync(r => r.SeasonId == id && r.TeamId == teamId);

            if (registration is null)
                return Results.Redirect($"/teams/{teamId}?error=not_registered");

            db.SeasonRegistrations.Remove(registration);
            await db.SaveChangesAsync();

            return Results.Redirect($"/teams/{teamId}");
        }

        // Admin: build the regular season schedule by queuing the BuildRegularSeason function
        private static async Task<IResult> BuildAsync(
            HttpContext context,
            ServiceBusClient serviceBusClient,
            long id)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            await using var sender = serviceBusClient.CreateSender("build-regular-season");

            var payload = JsonSerializer.Serialize(new BuildRegularSeasonMessage { SeasonId = id });

            await sender.SendMessageAsync(new ServiceBusMessage(payload));

            return Results.Redirect($"/seasons/{id}");
        }

        private static async Task<User?> GetCurrentUserAsync(HttpContext context, SqlDbContext db)
        {
            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (oid is null)
                return null;

            return await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);
        }
    }
}
