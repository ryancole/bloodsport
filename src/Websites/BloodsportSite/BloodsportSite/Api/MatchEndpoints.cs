using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;

namespace BloodsportSite.Api
{
    public static class MatchEndpoints
    {
        public static IEndpointRouteBuilder MapMatches(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/matches/{id}/report", ReportAsync)
                .RequireAuthorization()
                .DisableAntiforgery();

            return endpoints;
        }

        // Captain: report match result by submitting the Riot match ID
        private static async Task<IResult> ReportAsync(
            HttpContext context,
            IDbContextFactory<SqlDbContext> dbFactory,
            long id,
            [Microsoft.AspNetCore.Mvc.FromForm] string riotMatchId,
            [Microsoft.AspNetCore.Mvc.FromForm] long winnerTeamId)
        {
            riotMatchId = riotMatchId.Trim();

            if (string.IsNullOrEmpty(riotMatchId))
                return Results.BadRequest("Riot match ID is required.");

            await using var db = dbFactory.CreateDbContext();
            var user = await GetCurrentUserAsync(context, db);

            if (user is null)
                return Results.Unauthorized();

            var match = await db.Matches
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match is null)
                return Results.NotFound("Match not found.");

            if (match.Status != MatchStatus.Pending)
                return Results.BadRequest("Match has already been reported.");

            // Only the captain of one of the two teams may report
            var isHomeCaptain = match.HomeTeam.ManagerId == user.Id;
            var isAwayCaptain = match.AwayTeam.ManagerId == user.Id;

            if (!isHomeCaptain && !isAwayCaptain)
                return Results.Forbid();

            if (winnerTeamId != match.HomeTeamId && winnerTeamId != match.AwayTeamId)
                return Results.BadRequest("Winner must be one of the two teams in this match.");

            match.RiotMatchId = riotMatchId;
            match.WinnerTeamId = winnerTeamId;
            match.ReportedByUserId = user.Id;
            match.Status = MatchStatus.Completed;

            await db.SaveChangesAsync();

            return Results.Redirect($"/seasons/{match.SeasonId}");
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
