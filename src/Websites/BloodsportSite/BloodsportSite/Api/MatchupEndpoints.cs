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
                .RequireAuthorization()
                .DisableAntiforgery();

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
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matchup is null)
                return Results.Redirect($"?error=not_found");

            var isTeamOneManager = matchup.TeamOne.ManagerId == user.Id;
            var isTeamTwoManager = matchup.TeamTwo.ManagerId == user.Id;

            if (!isTeamOneManager && !isTeamTwoManager)
                return Results.Forbid();

            if (matchup.TournamentCode is not null)
                return Results.Redirect(MatchupUrl(matchup));

            var season = matchup.SeasonWeek.Season;

            if (season.RiotTournamentId is null)
                return Results.Redirect($"{MatchupUrl(matchup)}?error=tournament_not_configured");

            try
            {
                matchup.TournamentCode = await riotClient.CreateTournamentCodeAsync(season.RiotTournamentId.Value);
                await db.SaveChangesAsync();
            }
            catch (HttpRequestException)
            {
                return Results.Redirect($"{MatchupUrl(matchup)}?error=riot_api_error");
            }

            return Results.Redirect(MatchupUrl(matchup));
        }

        private static string MatchupUrl(SeasonWeekMatchup matchup) =>
            $"/seasons/{matchup.SeasonWeek.SeasonId}/weeks/{matchup.SeasonWeekId}/matchups/{matchup.Id}";
    }
}
