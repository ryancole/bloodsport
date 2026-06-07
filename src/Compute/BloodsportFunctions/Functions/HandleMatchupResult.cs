using System.Text.Json;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.RiotApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class HandleMatchupResult
{
    private readonly ILogger<HandleMatchupResult> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;

    public HandleMatchupResult(ILogger<HandleMatchupResult> logger, IDbContextFactory<SqlDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
    }

    [Function("HandleMatchupResult")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        RiotTournamentCallbackPayload? payload;

        try
        {
            payload = await JsonSerializer.DeserializeAsync<RiotTournamentCallbackPayload>(req.Body);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Riot callback payload.");
            return new BadRequestResult();
        }

        if (payload is null || string.IsNullOrEmpty(payload.ShortCode))
        {
            _logger.LogError("Riot callback payload was null or missing shortCode.");
            return new BadRequestResult();
        }

        var winningParticipants = payload.Participants.Where(p => p.Win).ToList();

        if (winningParticipants.Count == 0)
        {
            _logger.LogWarning("Riot callback for {shortCode} contained no winning participants.", payload.ShortCode);
            return new OkResult();
        }

        var winningPuuids = winningParticipants.Select(p => p.Puuid).ToHashSet();

        await using var db = await _dbFactory.CreateDbContextAsync();

        var matchup = await db.SeasonWeekMatchups
            .Include(m => m.SeasonWeek)
            .FirstOrDefaultAsync(m => m.TournamentCode == payload.ShortCode);

        if (matchup is null)
        {
            _logger.LogError("No matchup found for tournament code {shortCode}.", payload.ShortCode);
            return new NotFoundResult();
        }

        var existingResult = await db.SeasonWeekMatchupResults
            .FirstOrDefaultAsync(r => r.SeasonWeekMatchupId == matchup.Id);

        if (existingResult is not null)
        {
            _logger.LogWarning("Matchup {matchupId} already has a result recorded. Ignoring duplicate callback.", matchup.Id);
            return new OkResult();
        }

        // Find a team membership whose RiotAccount PUUID is in the winning side
        var winningMembership = await db.TeamMemberships
            .Include(tm => tm.RiotAccount)
            .Include(tm => tm.Team)
            .Where(tm =>
                (tm.TeamId == matchup.TeamOneId || tm.TeamId == matchup.TeamTwoId) &&
                winningPuuids.Contains(tm.RiotAccount.Puuid))
            .FirstOrDefaultAsync();

        if (winningMembership is null)
        {
            _logger.LogError(
                "Could not resolve a winning team for matchup {matchupId} from {count} winning PUUIDs.",
                matchup.Id, winningPuuids.Count);
            return new UnprocessableEntityResult();
        }

        db.SeasonWeekMatchupResults.Add(new Bloodsport.Entity.Database.SeasonWeekMatchupResult
        {
            SeasonWeekMatchupId = matchup.Id,
            SeasonWeekMatchup = matchup,
            WinnerTeamId = winningMembership.TeamId,
            WinnerTeam = winningMembership.Team,
        });

        var seasonId = matchup.SeasonWeek.SeasonId;
        var losingTeamId = matchup.TeamOneId == winningMembership.TeamId ? matchup.TeamTwoId : matchup.TeamOneId;

        var winnerResult = await db.TeamSeasonResults
            .FirstOrDefaultAsync(r => r.TeamId == winningMembership.TeamId && r.SeasonId == seasonId);

        var loserResult = await db.TeamSeasonResults
            .FirstOrDefaultAsync(r => r.TeamId == losingTeamId && r.SeasonId == seasonId);

        if (winnerResult is not null)
            winnerResult.WinCount++;

        if (loserResult is not null)
            loserResult.LoseCount++;

        await db.SaveChangesAsync();

        _logger.LogInformation(
            "Recorded winner team {teamId} for matchup {matchupId} (tournament code {shortCode}).",
            winningMembership.TeamId, matchup.Id, payload.ShortCode);

        return new OkResult();
    }
}
