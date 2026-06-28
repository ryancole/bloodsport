using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.ServiceBus;
using Bloodsport.Entity.Database;
using Camille.Enums;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class SeasonEndpoints
    {
        public static IEndpointRouteBuilder MapSeasons(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/seasons", ListAsync);

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

            endpoints.MapPost("/seasons/{id}/start", StartAsync)
                .RequireAuthorization();

            endpoints.MapPost("/seasons/{id}/create-playoffs", CreatePlayoffsAsync)
                .RequireAuthorization();

            return endpoints;
        }

        private static async Task<IResult> ListAsync(IDbContextFactory<SqlDbContext> dbFactory)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var seasons = await db.Seasons
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();

            var result = seasons.Select(s => new
            {
                s.Id,
                s.Name,
                s.Status,
                s.RegistrationOpen,
                s.EstimatedDateEnd,
                s.Length,
                s.DateCreated,
            });

            return Results.Ok(result);
        }

        // Admin: create a season
        private static async Task<IResult> CreateAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromForm] string name,
            [Microsoft.AspNetCore.Mvc.FromForm] int length = 6,
            [Microsoft.AspNetCore.Mvc.FromForm] string? startDate = null)
        {
            if (!context.User.IsInRole("Champions.Admin"))
                return Results.Forbid();

            name = name.Trim();

            if (string.IsNullOrEmpty(name))
                return Results.Redirect("/seasons/create?error=invalid_name");

            DateTime? parsedStartDate = DateTime.TryParse(startDate, out var d) ? d : null;

            await using var db = dbFactory.CreateDbContext();

            if (length < 3 || length > 8)
                length = 6;

            var season = new Season
            {
                Name = name,
                Length = length,
                EstimatedDateStart = parsedStartDate,
                RiotRegion = TournamentRegion.NA.ToString(),
            };

            db.Seasons.Add(season);
            await db.SaveChangesAsync();

            return Results.Redirect("/seasons");
        }

        // Admin: edit a season
        private static async Task<IResult> EditAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] SeasonStatus status,
            [Microsoft.AspNetCore.Mvc.FromForm] bool? registrationOpen = null,
            [Microsoft.AspNetCore.Mvc.FromForm] string? startDate = null)
        {
            if (!context.User.IsInRole("Champions.Admin"))
                return Results.Forbid();

            await using var db = dbFactory.CreateDbContext();
            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.Redirect("/seasons?error=season_not_found");

            season.Status = status;
            season.RegistrationOpen = registrationOpen ?? false;
            season.EstimatedDateStart = DateTime.TryParse(startDate, out var d) ? d : null;

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

            // The team is inaugural only if this is the first season it has ever
            // registered for; having played in any prior season disqualifies it.
            var hasPlayedPriorSeason = await db.SeasonRegistrations
                .AnyAsync(r => r.TeamId == teamId && r.SeasonId != id);

            db.SeasonRegistrations.Add(new SeasonRegistration
            {
                SeasonId = id,
                Season = season,
                TeamId = teamId,
                Team = team,
                InauguralRegistration = !hasPlayedPriorSeason,
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
            if (!context.User.IsInRole("Champions.Admin"))
                return Results.Forbid();

            await using var sender = serviceBusClient.CreateSender("build-regular-season");

            var payload = JsonSerializer.Serialize(new BuildRegularSeasonMessage { SeasonId = id });

            await sender.SendMessageAsync(new ServiceBusMessage(payload));

            return Results.Redirect($"/seasons/{id}");
        }

        // Admin: start the regular season by queuing the StartRegularSeason function
        private static async Task<IResult> StartAsync(
            HttpContext context,
            ServiceBusClient serviceBusClient,
            long id)
        {
            if (!context.User.IsInRole("Champions.Admin"))
                return Results.Forbid();

            await using var sender = serviceBusClient.CreateSender("start-regular-season");

            var payload = JsonSerializer.Serialize(new StartRegularSeasonMessage { SeasonId = id });

            await sender.SendMessageAsync(new ServiceBusMessage(payload));

            return Results.Redirect($"/seasons/{id}");
        }

        // Admin: create a Playoff record for a completed season and redirect to its detail page
        private static async Task<IResult> CreatePlayoffsAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id)
        {
            if (!context.User.IsInRole("Champions.Admin"))
                return Results.Forbid();

            await using var db = dbFactory.CreateDbContext();

            var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id);

            if (season is null)
                return Results.Redirect("/seasons?error=season_not_found");

            if (season.Status != SeasonStatus.Completed)
                return Results.Redirect($"/seasons/{id}?error=invalid_season_status");

            var playoff = new Playoff
            {
                SeasonId = id,
                Season = season,
                Name = $"{season.Name} Playoffs",
                Status = PlayoffStatus.Upcoming,
                RiotRegion = TournamentRegion.NA.ToString(),
            };

            db.Playoffs.Add(playoff);
            await db.SaveChangesAsync();

            return Results.Redirect($"/playoffs/{playoff.Id}");
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
