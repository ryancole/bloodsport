using System.Text.Json;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.RiotApi;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.TournamentV5;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class HandleSeasonWeekMatchup
{
    private readonly ILogger<HandleSeasonWeekMatchup> _logger;
    private readonly IDbContextFactory<SqlDbContext> _dbFactory;
    private readonly RiotGamesApi _riotApi;
    private readonly IConfiguration _config;

    public HandleSeasonWeekMatchup(
        ILogger<HandleSeasonWeekMatchup> logger,
        IDbContextFactory<SqlDbContext> dbFactory,
        RiotGamesApi riotApi,
        IConfiguration config)
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _riotApi = riotApi;
        _config = config;
    }

    private RegionalRoute Route =>
        Enum.Parse<RegionalRoute>(_config["RiotApi:RegionalRoute"] ?? "AMERICAS", ignoreCase: true);

    private string PlatformId => _config["RiotApi:PlatformId"] ?? "NA1";

    [Function("HandleSeasonWeekMatchup")]
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

        var seasonId = matchup.SeasonWeek.SeasonId;

        var participantAccounts = await db.RiotAccounts
            .Where(a => allPuuids.Contains(a.Puuid))
            .ToListAsync();

        var teamOneRoster = await db.TeamSeasonRosters
            .FirstOrDefaultAsync(r => r.TeamId == matchup.TeamOneId && r.SeasonId == seasonId);

        var teamTwoRoster = await db.TeamSeasonRosters
            .FirstOrDefaultAsync(r => r.TeamId == matchup.TeamTwoId && r.SeasonId == seasonId);

        var teamOneAllowed = teamOneRoster is not null
            ? (JsonSerializer.Deserialize<TeamSeasonRosterJson>(teamOneRoster.RosterJson)?.AllowedSummonerNames ?? [])
            : (ICollection<string>)[];

        var teamTwoAllowed = teamTwoRoster is not null
            ? (JsonSerializer.Deserialize<TeamSeasonRosterJson>(teamTwoRoster.RosterJson)?.AllowedSummonerNames ?? [])
            : (ICollection<string>)[];

        // Winning team maps to the team we resolved as winner; losing team maps to the other.
        var losingTeamId = matchup.TeamOneId == winningMembership.TeamId ? matchup.TeamTwoId : matchup.TeamOneId;
        var winnerAllowed = winningMembership.TeamId == matchup.TeamOneId ? teamOneAllowed : teamTwoAllowed;
        var loserAllowed = losingTeamId == matchup.TeamOneId ? teamOneAllowed : teamTwoAllowed;

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

        var winnerTeamAverageGpm = await TryGetWinnerAverageGpmAsync(payload, winningPuuids);

        db.SeasonWeekMatchupResults.Add(new SeasonWeekMatchupResult
        {
            SeasonWeekMatchupId = matchup.Id,
            SeasonWeekMatchup = matchup,
            WinnerTeamId = winningMembership.TeamId,
            WinnerTeam = winningMembership.Team,
            RosterMismatch = rosterMismatch,
            WinnerTeamAverageGPM = winnerTeamAverageGpm,
        });

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
            "Recorded winner team {teamId} for matchup {matchupId} (tournament code {shortCode}), winner GPM: {gpm}.",
            winningMembership.TeamId, matchup.Id, payload.ShortCode, winnerTeamAverageGpm);

        return new OkResult();
    }

    private async Task<long> TryGetWinnerAverageGpmAsync(TournamentGamesV5 payload, HashSet<string> winningPuuids)
    {
        if (RiotApiEndpoints.UseStub || payload.GameId == 0)
            return 0;

        try
        {
            var matchId = $"{PlatformId}_{payload.GameId}";
            var match = await _riotApi.MatchV5().GetMatchAsync(Route, matchId);

            if (match?.Info is null)
            {
                _logger.LogWarning("Match {matchId} returned null info; winner GPM will be 0.", matchId);
                return 0;
            }

            var gameDurationMinutes = match.Info.GameDuration / 60.0;
            if (gameDurationMinutes <= 0)
                return 0;

            var totalGold = match.Info.Participants
                .Where(p => winningPuuids.Contains(p.Puuid))
                .Sum(p => (long)p.GoldEarned);

            return (long)(totalGold / gameDurationMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch match stats for game {gameId}; winner GPM will be 0.", payload.GameId);
            return 0;
        }
    }
}
