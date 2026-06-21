using System.Text.Json;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using BloodsportFunctions.Services;
using Camille.RiotGames.TournamentV5;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class HandlePlayoffMatchup
{
    private readonly ILogger<HandlePlayoffMatchup> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;
    private readonly EmailService _emailService;

    public HandlePlayoffMatchup(ILogger<HandlePlayoffMatchup> logger, IDbContextFactory<SqlDbContext> dbFactory, EmailService emailService)
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _emailService = emailService;
    }

    [Function("HandlePlayoffMatchup")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        TournamentGamesV5? payload;

        try
        {
            payload = await JsonSerializer.DeserializeAsync<TournamentGamesV5>(req.Body);
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

        if (payload.WinningTeam.Length == 0)
        {
            _logger.LogWarning("Riot callback for {shortCode} contained no winning team participants.", payload.ShortCode);
            return new OkResult();
        }

        var winningPuuids = payload.WinningTeam.Select(p => p.Puuid).ToHashSet();
        var losingPuuids = payload.LosingTeam.Select(p => p.Puuid).ToHashSet();
        var allPuuids = winningPuuids.Concat(losingPuuids).ToHashSet();

        await using var db = await _dbFactory.CreateDbContextAsync();

        var matchup = await db.PlayoffRoundMatchups
            .Include(m => m.PlayoffRound).ThenInclude(r => r.Playoff)
            .Include(m => m.TeamOne).ThenInclude(pt => pt!.Team)
            .Include(m => m.TeamTwo).ThenInclude(pt => pt!.Team)
            .FirstOrDefaultAsync(m => m.TournamentCode == payload.ShortCode);

        if (matchup is null)
        {
            _logger.LogError("No playoff matchup found for tournament code {shortCode}.", payload.ShortCode);
            return new NotFoundResult();
        }

        if (matchup.WinningTeamId is not null)
        {
            _logger.LogWarning("Playoff matchup {matchupId} already has a result recorded. Ignoring duplicate callback.", matchup.Id);
            return new OkResult();
        }

        // Resolve winner by finding a team membership whose PUUID is in the winning set.
        var teamOneId = matchup.TeamOne?.TeamId;
        var teamTwoId = matchup.TeamTwo?.TeamId;

        var winningMembership = await db.TeamMemberships
            .Include(tm => tm.RiotAccount)
            .Include(tm => tm.Team)
            .Where(tm =>
                (tm.TeamId == teamOneId || tm.TeamId == teamTwoId) &&
                winningPuuids.Contains(tm.RiotAccount.Puuid))
            .FirstOrDefaultAsync();

        if (winningMembership is null)
        {
            _logger.LogError(
                "Could not resolve a winning team for playoff matchup {matchupId} from {count} winning PUUIDs.",
                matchup.Id, winningPuuids.Count);
            return new UnprocessableEntityResult();
        }

        // Map the winning Team back to its PlayoffTeam record.
        var winningPlayoffTeam = winningMembership.TeamId == teamOneId ? matchup.TeamOne : matchup.TeamTwo;
        var losingPlayoffTeam  = winningMembership.TeamId == teamOneId ? matchup.TeamTwo : matchup.TeamOne;

        // Roster validation against season rosters.
        var seasonId = matchup.PlayoffRound.Playoff.SeasonId;

        var participantAccounts = await db.RiotAccounts
            .Where(a => allPuuids.Contains(a.Puuid))
            .ToListAsync();

        var teamOneRoster = await db.TeamSeasonRosters
            .FirstOrDefaultAsync(r => r.TeamId == teamOneId && r.SeasonId == seasonId);

        var teamTwoRoster = await db.TeamSeasonRosters
            .FirstOrDefaultAsync(r => r.TeamId == teamTwoId && r.SeasonId == seasonId);

        var teamOneAllowed = teamOneRoster is not null
            ? (JsonSerializer.Deserialize<TeamSeasonRosterJson>(teamOneRoster.RosterJson)?.AllowedSummonerNames ?? [])
            : (ICollection<string>)[];

        var teamTwoAllowed = teamTwoRoster is not null
            ? (JsonSerializer.Deserialize<TeamSeasonRosterJson>(teamTwoRoster.RosterJson)?.AllowedSummonerNames ?? [])
            : (ICollection<string>)[];

        var winnerAllowed = winningMembership.TeamId == teamOneId ? teamOneAllowed : teamTwoAllowed;
        var loserAllowed  = winningMembership.TeamId == teamOneId ? teamTwoAllowed : teamOneAllowed;

        var rosterMismatch = false;

        foreach (var puuid in winningPuuids.Concat(losingPuuids))
        {
            var account = participantAccounts.FirstOrDefault(a => a.Puuid == puuid);
            if (account is null)
            {
                rosterMismatch = true;
                break;
            }

            var summonerName = $"{account.GameName}#{account.TagLine}";
            var allowed = winningPuuids.Contains(puuid) ? winnerAllowed : loserAllowed;

            if (!allowed.Contains(summonerName))
            {
                rosterMismatch = true;
                break;
            }
        }

        // Record the result on the matchup.
        matchup.WinningTeamId = winningPlayoffTeam!.Id;

        // Advance the winner into the next round's matchup slot.
        // Even MatchNumber → winner becomes TeamOne; odd → TeamTwo.
        if (matchup.NextMatchupId is not null)
        {
            var nextMatchup = await db.PlayoffRoundMatchups.FirstAsync(m => m.Id == matchup.NextMatchupId);

            if (matchup.MatchNumber % 2 == 0)
                nextMatchup.TeamOneId = winningPlayoffTeam.Id;
            else
                nextMatchup.TeamTwoId = winningPlayoffTeam.Id;
        }
        else
        {
            // No next matchup means this is the grand final.
            matchup.PlayoffRound.Playoff.Status = PlayoffStatus.Completed;
        }

        await db.SaveChangesAsync();

        if (matchup.NextMatchupId is null)
        {
            await SendPlayoffEndedEmailsAsync(db, matchup);
        }

        _logger.LogInformation(
            "Playoff matchup {matchupId} (tournament code {shortCode}): winner {teamName} (team {teamId}, playoff team {playoffTeamId}). RosterMismatch={rosterMismatch}.",
            matchup.Id, payload.ShortCode,
            winningMembership.Team.Name, winningMembership.TeamId, winningPlayoffTeam.Id,
            rosterMismatch);

        return new OkResult();
    }

    private async Task SendPlayoffEndedEmailsAsync(SqlDbContext db, PlayoffRoundMatchup finalMatchup)
    {
        var playoff = finalMatchup.PlayoffRound.Playoff;

        var allMatchups = await db.PlayoffRoundMatchups
            .Include(m => m.PlayoffRound)
            .Include(m => m.TeamOne)
                .ThenInclude(pt => pt!.Team)
                    .ThenInclude(t => t.TeamMemberships)
                        .ThenInclude(tm => tm.RiotAccount)
                            .ThenInclude(a => a.User)
            .Include(m => m.TeamTwo)
                .ThenInclude(pt => pt!.Team)
                    .ThenInclude(t => t.TeamMemberships)
                        .ThenInclude(tm => tm.RiotAccount)
                            .ThenInclude(a => a.User)
            .Where(m => m.PlayoffRound.PlayoffId == playoff.Id)
            .ToListAsync();

        // Build a lookup of PlayoffTeam.Id → the round name in which they were eliminated.
        var eliminatedInRound = new Dictionary<long, string>();
        foreach (var m in allMatchups.Where(m => m.WinningTeamId is not null))
        {
            var loserId = m.TeamOneId == m.WinningTeamId ? m.TeamTwoId : m.TeamOneId;
            if (loserId is not null)
                eliminatedInRound[loserId.Value] = m.PlayoffRound.Name;
        }

        // The final matchup has no NextMatchupId; its winner is 1st, loser is 2nd.
        var winnerId = finalMatchup.WinningTeamId!.Value;
        var loserId2nd = finalMatchup.TeamOneId == winnerId ? finalMatchup.TeamTwoId : finalMatchup.TeamOneId;

        var allPlayoffTeams = allMatchups
            .SelectMany(m => new[] { m.TeamOne, m.TeamTwo })
            .Where(pt => pt is not null)
            .DistinctBy(pt => pt!.Id)
            .Select(pt => pt!)
            .ToList();

        var teamResults = allPlayoffTeams.Select(pt =>
        {
            string finish;
            if (pt.Id == winnerId)
                finish = "1st place";
            else if (pt.Id == loserId2nd)
                finish = "2nd place";
            else if (eliminatedInRound.TryGetValue(pt.Id, out var roundName))
                finish = $"eliminated in the {roundName}";
            else
                finish = "eliminated";

            var members = pt.Team.TeamMemberships
                .Select(tm => tm.RiotAccount.User)
                .DistinctBy(u => u.Id);

            return (pt.Team, finish, members);
        });

        await _emailService.SendPlayoffEndedAsync(playoff, teamResults);
    }
}
