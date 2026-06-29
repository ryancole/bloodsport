using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using BloodsportSite.Services;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class MatchupEndpoints
    {
        public static IEndpointRouteBuilder MapMatchups(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/matchups/{id}/tournament-code", RequestTournamentCodeAsync)
                .RequireAuthorization();

            endpoints.MapPost("/matchups/{id}/fetch-lobby-events", FetchLobbyEventsAsync)
                .RequireAuthorization();

            endpoints.MapPost("/matchups/{id}/forfeit", ForfeitAsync)
                .RequireAuthorization();

            return endpoints;
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
                return Results.Redirect($"?error=not_authenticated");

            var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);
            if (user is null)
                return Results.Redirect($"?error=not_authenticated");

            var matchup = await db.SeasonWeekMatchups
                .Include(m => m.TeamOne)
                .Include(m => m.TeamTwo)
                .Include(m => m.SeasonWeek)
                    .ThenInclude(w => w.Season)
                        .ThenInclude(s => s.MatchupParameters)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matchup is null)
                return Results.Redirect($"?error=not_found");

            var isTeamOneManager = matchup.TeamOne.ManagerId == user.Id;
            var isTeamTwoManager = matchup.TeamTwo.ManagerId == user.Id;

            if (!isTeamOneManager && !isTeamTwoManager)
                return Results.Forbid();

            if (matchup.TournamentCode is not null)
                return Results.Redirect(MatchupUrl(matchup));

            if (matchup.SeasonWeek.DateEnd < DateTime.UtcNow)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=week_ended");

            var season = matchup.SeasonWeek.Season;

            if (season.RiotTournamentId is null)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=tournament_not_configured");

            var rosters = await db.TeamSeasonRosters
                .Where(r => r.SeasonId == season.Id && (r.TeamId == matchup.TeamOneId || r.TeamId == matchup.TeamTwoId))
                .ToListAsync();

            var allowedSummonerNames = rosters
                .SelectMany(r => JsonSerializer.Deserialize<TeamSeasonRosterJson>(r.RosterJson)?.AllowedSummonerNames ?? [])
                .ToHashSet();

            var allowedPuuids = await db.RiotAccounts
                .Where(a => allowedSummonerNames.Contains(a.GameName + "#" + a.TagLine) && a.Puuid != null && a.Puuid != "" && !a.Puuid.StartsWith("FAKE-"))
                .Select(a => a.Puuid)
                .ToArrayAsync();

            try
            {
                matchup.TournamentCode = await riotClient.CreateTournamentCodeAsync(
                    season.RiotTournamentId.Value,
                    parameters: season.MatchupParameters,
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

            var matchup = await db.SeasonWeekMatchups
                .Include(m => m.TeamOne)
                .Include(m => m.TeamTwo)
                .Include(m => m.SeasonWeek)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matchup is null)
                return Results.Redirect("?error=not_found");

            if (matchup.TeamOne.ManagerId != user.Id && matchup.TeamTwo.ManagerId != user.Id)
                return Results.Forbid();

            if (matchup.TournamentCode is null)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=no_tournament_code");

            await using var sender = serviceBusClient.CreateSender("fetch-riot-lobby-events");
            await sender.SendMessageAsync(new ServiceBusMessage(matchup.TournamentCode));

            return Results.Redirect($"{MatchupUrl(matchup)}?info=lobby_events_requested");
        }

        private static async Task<IResult> ForfeitAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
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

            var matchup = await db.SeasonWeekMatchups
                .Include(m => m.TeamOne)
                .Include(m => m.TeamTwo)
                .Include(m => m.SeasonWeek)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matchup is null)
                return Results.Redirect("?error=not_found");

            var isTeamOneManager = matchup.TeamOne.ManagerId == user.Id;
            var isTeamTwoManager = matchup.TeamTwo.ManagerId == user.Id;

            if (!isTeamOneManager && !isTeamTwoManager)
                return Results.Forbid();

            if (matchup.SeasonWeek.HasBeenEnded)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=week_ended");

            var existingResult = await db.SeasonWeekMatchupResults
                .FirstOrDefaultAsync(r => r.SeasonWeekMatchupId == matchup.Id);

            if (existingResult is not null)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=result_exists");

            var forfeitingTeamId = isTeamOneManager ? matchup.TeamOneId : matchup.TeamTwoId;
            var winningTeamId = isTeamOneManager ? matchup.TeamTwoId : matchup.TeamOneId;
            var winningTeam = isTeamOneManager ? matchup.TeamTwo : matchup.TeamOne;

            var seasonId = matchup.SeasonWeek.SeasonId;

            db.SeasonWeekMatchupResults.Add(new SeasonWeekMatchupResult
            {
                SeasonWeekMatchupId = matchup.Id,
                SeasonWeekMatchup = matchup,
                WinnerTeamId = winningTeamId,
                WinnerTeam = winningTeam,
                DidNotPlay = true,
                WinnerTeamAverageGPM = 0,
            });

            var winnerResult = await db.TeamSeasonResults
                .FirstOrDefaultAsync(r => r.TeamId == winningTeamId && r.SeasonId == seasonId);

            var loserResult = await db.TeamSeasonResults
                .FirstOrDefaultAsync(r => r.TeamId == forfeitingTeamId && r.SeasonId == seasonId);

            if (winnerResult is not null)
                winnerResult.WinCount++;

            if (loserResult is not null)
                loserResult.LoseCount++;

            await db.SaveChangesAsync();

            return Results.Redirect($"{MatchupUrl(matchup)}?info=forfeit_recorded");
        }

        private static string MatchupUrl(SeasonWeekMatchup matchup) =>
            $"/seasons/{matchup.SeasonWeek.SeasonId}/weeks/{matchup.SeasonWeekId}/matchups/{matchup.Id}";
    }
}
