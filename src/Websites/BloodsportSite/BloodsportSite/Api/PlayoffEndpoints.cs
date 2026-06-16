using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.ServiceBus;
using BloodsportSite.Services;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class PlayoffEndpoints
    {
        public static IEndpointRouteBuilder MapPlayoffs(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/playoffs/{id}/build-bracket", BuildBracketAsync)
                .RequireAuthorization();

            endpoints.MapPost("/playoffs/{id}/start", StartPlayoffAsync)
                .RequireAuthorization();

            endpoints.MapPost("/playoff-matchups/{id}/tournament-code", RequestTournamentCodeAsync)
                .RequireAuthorization();

            endpoints.MapPost("/playoff-matchups/{id}/fetch-lobby-events", FetchLobbyEventsAsync)
                .RequireAuthorization();

            return endpoints;
        }

        // Admin: queue the StartPlayoff function for a playoff
        private static async Task<IResult> StartPlayoffAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            ServiceBusClient serviceBusClient,
            long id)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            await using var db = dbFactory.CreateDbContext();

            var playoff = await db.Playoffs.FirstOrDefaultAsync(p => p.Id == id);

            if (playoff is null)
                return Results.Redirect("/playoffs?error=playoff_not_found");

            await using var sender = serviceBusClient.CreateSender("start-playoff");

            var payload = JsonSerializer.Serialize(new StartPlayoffMessage { PlayoffId = id });

            await sender.SendMessageAsync(new ServiceBusMessage(payload));

            return Results.Redirect($"/playoffs/{id}");
        }

        private static async Task<IResult> RequestTournamentCodeAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            RiotTournamentClient riotClient,
            long id)
        {
            await using var db = dbFactory.CreateDbContext();

            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (oid is null)
                return Results.Redirect("?error=not_authenticated");

            var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);
            if (user is null)
                return Results.Redirect("?error=not_authenticated");

            var matchup = await db.PlayoffMatchups
                .Include(m => m.TeamOne).ThenInclude(pt => pt!.Team)
                .Include(m => m.TeamTwo).ThenInclude(pt => pt!.Team)
                .Include(m => m.PlayoffRound).ThenInclude(r => r.Playoff)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matchup is null)
                return Results.Redirect("?error=not_found");

            var isTeamOneManager = matchup.TeamOne?.Team?.ManagerId == user.Id;
            var isTeamTwoManager = matchup.TeamTwo?.Team?.ManagerId == user.Id;

            if (!isTeamOneManager && !isTeamTwoManager)
                return Results.Forbid();

            if (matchup.TournamentCode is not null)
                return Results.Redirect(MatchupUrl(matchup));

            if (matchup.PlayoffRound.DateEnd.HasValue && matchup.PlayoffRound.DateEnd.Value < DateTime.UtcNow)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=round_ended");

            var playoff = matchup.PlayoffRound.Playoff;

            if (playoff.RiotTournamentId is null)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=tournament_not_configured");

            var teamOneId = matchup.TeamOne?.TeamId;
            var teamTwoId = matchup.TeamTwo?.TeamId;

            var rosters = await db.TeamPlayoffRosters
                .Where(r => r.PlayoffId == playoff.Id && (r.TeamId == teamOneId || r.TeamId == teamTwoId))
                .ToListAsync();

            var allowedSummonerNames = rosters
                .SelectMany(r => JsonSerializer.Deserialize<TeamPlayoffRosterJson>(r.RosterJson)?.AllowedSummonerNames ?? [])
                .ToHashSet();

            var allowedPuuids = await db.RiotAccounts
                .Where(a => allowedSummonerNames.Contains(a.GameName + "#" + a.TagLine))
                .Select(a => a.Puuid)
                .ToArrayAsync();

            try
            {
                matchup.TournamentCode = await riotClient.CreateTournamentCodeAsync(
                    playoff.RiotTournamentId.Value,
                    allowedParticipants: allowedPuuids.Length > 0 ? allowedPuuids : null);
                await db.SaveChangesAsync();
            }
            catch (HttpRequestException)
            {
                return Results.Redirect($"{MatchupUrl(matchup)}?error=riot_api_error");
            }

            return Results.Redirect(MatchupUrl(matchup));
        }

        private static async Task<IResult> FetchLobbyEventsAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            ServiceBusClient serviceBusClient,
            long id)
        {
            await using var db = dbFactory.CreateDbContext();

            var oid = context.User.FindFirst("oid")?.Value
                   ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (oid is null)
                return Results.Redirect("?error=not_authenticated");

            var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);
            if (user is null)
                return Results.Redirect("?error=not_authenticated");

            var matchup = await db.PlayoffMatchups
                .Include(m => m.TeamOne).ThenInclude(pt => pt!.Team)
                .Include(m => m.TeamTwo).ThenInclude(pt => pt!.Team)
                .Include(m => m.PlayoffRound).ThenInclude(r => r.Playoff)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matchup is null)
                return Results.Redirect("?error=not_found");

            if (matchup.TeamOne?.Team?.ManagerId != user.Id && matchup.TeamTwo?.Team?.ManagerId != user.Id)
                return Results.Forbid();

            if (matchup.TournamentCode is null)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=no_tournament_code");

            await using var sender = serviceBusClient.CreateSender("fetch-riot-lobby-events");
            await sender.SendMessageAsync(new ServiceBusMessage(matchup.TournamentCode));

            return Results.Redirect($"{MatchupUrl(matchup)}?info=lobby_events_requested");
        }

        private static string MatchupUrl(PlayoffMatchup matchup) =>
            $"/playoffs/{matchup.PlayoffRound.Playoff.Id}/matchups/{matchup.Id}";

        // Admin: queue the BuildPlayoffBracket function for a playoff
        private static async Task<IResult> BuildBracketAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            ServiceBusClient serviceBusClient,
            long id)
        {
            if (!context.User.IsInRole("Bloodsport.Admin"))
                return Results.Forbid();

            await using var db = dbFactory.CreateDbContext();

            var playoff = await db.Playoffs.FirstOrDefaultAsync(p => p.Id == id);

            if (playoff is null)
                return Results.Redirect("/playoffs?error=playoff_not_found");

            await using var sender = serviceBusClient.CreateSender("build-playoff-bracket");

            var payload = JsonSerializer.Serialize(new BuildPlayoffBracketMessage { PlayoffId = id });

            await sender.SendMessageAsync(new ServiceBusMessage(payload));

            return Results.Redirect($"/playoffs/{id}");
        }
    }
}
