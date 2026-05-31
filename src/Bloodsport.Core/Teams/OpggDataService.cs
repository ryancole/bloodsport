namespace Bloodsport.Core.Teams;

/// <summary>
/// Fetches player statistical profiles from op.gg and the Riot API.
///
/// Data sources:
///   Primary:   Riot API — match history, champion stats, ranked data
///   Secondary: op.gg Game Impact score, role distribution, champion grades
///
/// The goal is to build a PlayerProfile that reflects how a player
/// actually performs — not how they think they perform. The data
/// does not lie. The bracket does not care about your opinion of yourself.
///
/// Ryan: wire HTTP calls to real endpoints when Riot API key arrives.
/// The interface is already defined. Mock implementation below for beta.
/// </summary>
public interface IPlayerDataService
{
    Task<PlayerProfile> FetchProfileAsync(string summonerName, string region = "NA1");
    Task<PlayerProfile> FetchProfileByPuuidAsync(string puuid, string region = "NA1");
}

public class OpggDataService : IPlayerDataService
{
    private readonly HttpClient _riotClient;
    private readonly HttpClient _opggClient;

    // Riot API base URLs
    private const string RiotAccountBase  = "https://americas.api.riotgames.com";
    private const string RiotRegionalBase = "https://na1.api.riotgames.com";
    private const int    MatchHistoryCount = 50; // games to analyze per player

    public OpggDataService(IHttpClientFactory factory)
    {
        _riotClient = factory.CreateClient("riot");
        _opggClient = factory.CreateClient("opgg");
    }

    /// <summary>
    /// Builds a full PlayerProfile from 50 recent ranked games.
    /// All statistical dimensions are derived from real match data.
    /// </summary>
    public async Task<PlayerProfile> FetchProfileAsync(string summonerName, string region = "NA1")
    {
        // Step 1: Get PUUID from summoner name
        var summoner = await GetSummonerAsync(summonerName);
        if (summoner == null)
            return PlayerProfile_NotFound(summonerName);

        return await FetchProfileByPuuidAsync(summoner.Puuid, region);
    }

    public async Task<PlayerProfile> FetchProfileByPuuidAsync(string puuid, string region = "NA1")
    {
        // Step 2: Get recent ranked match IDs
        var matchIds = await GetRankedMatchIdsAsync(puuid, MatchHistoryCount);
        if (!matchIds.Any())
            return PlayerProfile_Insufficient(puuid);

        // Step 3: Fetch match details and extract performance data
        var matchDetails = new List<MatchDetail>();
        foreach (var matchId in matchIds)
        {
            var detail = await GetMatchDetailAsync(matchId, puuid);
            if (detail != null) matchDetails.Add(detail);
        }

        if (matchDetails.Count < 10)
            return PlayerProfile_Insufficient(puuid);

        // Step 4: Build profile from aggregated match data
        return BuildProfile(puuid, matchDetails);
    }

    // ── Profile Builder ──────────────────────────────────────────────────
    private PlayerProfile BuildProfile(string puuid, List<MatchDetail> matches)
    {
        var profile = new PlayerProfile
        {
            SummonerName       = matches.First().SummonerName,
            RankedGamesAnalyzed = matches.Count,
            ProfileBuiltAt     = DateTime.UtcNow,
            DataSource         = "riot_api"
        };

        // Role proficiency — normalized by games played per role × win rate
        var roleGroups = matches.GroupBy(m => m.Role).ToList();
        double totalGames = matches.Count;
        foreach (var group in roleGroups)
        {
            double playRate = group.Count() / totalGames;
            double winRate  = group.Count(m => m.Won) / (double)group.Count();
            profile.RoleProficiency[group.Key] = Math.Clamp(playRate * 0.6 + winRate * 0.4, 0, 1);
        }

        // Core stats
        profile.AverageKDA          = matches.Average(m => m.KDA);
        profile.AverageCSPerMin     = matches.Average(m => m.CSPerMinute);
        profile.AverageVisionScore  = matches.Average(m => m.VisionScorePerMinute);
        profile.AverageDamageShare  = matches.Average(m => m.DamageShare);
        profile.WinRateLast30       = matches.Take(30).Count(m => m.Won) / (double)Math.Min(30, matches.Count);

        // Playstyle vector — derived from statistical patterns
        profile.AggressionIndex   = NormalizeToUnit(matches.Average(m => m.KDA), 1.5, 6.0);
        profile.UtilityIndex      = NormalizeToUnit(matches.Average(m => m.AssistShare), 0.1, 0.6);
        profile.CarryCapacity     = NormalizeToUnit(matches.Average(m => m.DamageShare), 0.10, 0.35);
        profile.ObjectiveFocus    = NormalizeToUnit(matches.Average(m => m.ObjectiveParticipation), 0.3, 0.9);
        profile.ConsistencyRating = ComputeConsistency(matches.Select(m => m.PerformanceScore));

        // Champion pool tags — aggregate champion identities
        profile.EngagePotential    = matches.Average(m => m.ChampionEngageRating);
        profile.PeelPotential      = matches.Average(m => m.ChampionPeelRating);
        profile.PokePotential      = matches.Average(m => m.ChampionPokeRating);
        profile.SplitPushPotential = matches.Average(m => m.ChampionSplitRating);
        profile.TeamfightPotential = matches.Average(m => m.ChampionTeamfightRating);

        return profile;
    }

    // ── Riot API Calls ───────────────────────────────────────────────────
    // Ryan: implement these when Tournament API key and standard Riot key available.
    // The return types and method signatures are final — only the HTTP calls change.

    private async Task<SummonerDto?> GetSummonerAsync(string summonerName)
    {
        // GET /lol/summoner/v4/summoners/by-name/{summonerName}
        var response = await _riotClient.GetAsync(
            $"{RiotRegionalBase}/lol/summoner/v4/summoners/by-name/{Uri.EscapeDataString(summonerName)}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SummonerDto>();
    }

    private async Task<List<string>> GetRankedMatchIdsAsync(string puuid, int count)
    {
        // GET /lol/match/v5/matches/by-puuid/{puuid}/ids?queue=420&count={count}
        // queue=420 is RANKED_SOLO_5x5 — we only analyze ranked games
        var response = await _riotClient.GetAsync(
            $"{RiotAccountBase}/lol/match/v5/matches/by-puuid/{puuid}/ids?queue=420&count={count}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<string>>() ?? new();
    }

    private async Task<MatchDetail?> GetMatchDetailAsync(string matchId, string puuid)
    {
        // GET /lol/match/v5/matches/{matchId}
        var response = await _riotClient.GetAsync(
            $"{RiotAccountBase}/lol/match/v5/matches/{matchId}");
        if (!response.IsSuccessStatusCode) return null;

        var match = await response.Content.ReadFromJsonAsync<RiotMatchDto>();
        return match == null ? null : ExtractPlayerDetail(match, puuid);
    }

    private static MatchDetail? ExtractPlayerDetail(RiotMatchDto match, string puuid)
    {
        var participant = match.Info?.Participants?.FirstOrDefault(p => p.Puuid == puuid);
        if (participant == null) return null;

        double gameMins = (match.Info?.GameDuration ?? 0) / 60.0;
        double teamDamage = match.Info?.Participants?
            .Where(p => p.TeamId == participant.TeamId)
            .Sum(p => p.TotalDamageDealtToChampions) ?? 1;

        int teamObjectives = match.Info?.Teams?
            .FirstOrDefault(t => t.TeamId == participant.TeamId)
            ?.Objectives?.All?.Kills ?? 0;

        return new MatchDetail
        {
            MatchId                 = match.Metadata?.MatchId ?? string.Empty,
            Puuid                   = puuid,
            SummonerName            = participant.SummonerName,
            Won                     = participant.Win,
            Role                    = NormalizeRole(participant.TeamPosition),
            ChampionId              = participant.ChampionName,
            KDA                     = participant.Deaths == 0
                                        ? participant.Kills + participant.Assists
                                        : (participant.Kills + participant.Assists) / (double)participant.Deaths,
            CSPerMinute             = gameMins > 0 ? (participant.TotalMinionsKilled + participant.NeutralMinionsKilled) / gameMins : 0,
            VisionScorePerMinute    = gameMins > 0 ? participant.VisionScore / gameMins : 0,
            DamageShare             = teamDamage > 0 ? participant.TotalDamageDealtToChampions / teamDamage : 0,
            AssistShare             = (participant.Kills + participant.Assists) > 0
                                        ? participant.Assists / (double)(participant.Kills + participant.Assists) : 0,
            ObjectiveParticipation  = teamObjectives > 0 ? participant.ObjectivesStolenAssists / (double)teamObjectives : 0,
            PerformanceScore        = participant.ChallengeData?.GameScore ?? 50,
            // Champion type tags are seeded from a champion database
            // Ryan: wire ChampionTagDatabase.GetTags(championId) here
            ChampionEngageRating    = 0.5,
            ChampionPeelRating      = 0.5,
            ChampionPokeRating      = 0.5,
            ChampionSplitRating     = 0.5,
            ChampionTeamfightRating = 0.5
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    private static string NormalizeRole(string? teamPosition) => teamPosition switch
    {
        "TOP"     => "TOP",
        "JUNGLE"  => "JUNGLE",
        "MIDDLE"  => "MID",
        "BOTTOM"  => "ADC",
        "UTILITY" => "SUPPORT",
        _         => "MID"
    };

    private static double NormalizeToUnit(double value, double min, double max) =>
        Math.Clamp((value - min) / (max - min), 0, 1);

    private static double ComputeConsistency(IEnumerable<double> scores)
    {
        var list = scores.ToList();
        if (list.Count < 2) return 0.5;
        double mean = list.Average();
        double stdDev = Math.Sqrt(list.Average(s => Math.Pow(s - mean, 2)));
        double cv = mean > 0 ? stdDev / mean : 1.0;
        return Math.Clamp(1.0 - cv, 0, 1);
    }

    private static PlayerProfile PlayerProfile_NotFound(string name) => new()
    {
        SummonerName = name, DataSource = "not_found",
        ProfileBuiltAt = DateTime.UtcNow
    };

    private static PlayerProfile PlayerProfile_Insufficient(string puuid) => new()
    {
        SummonerName = puuid, DataSource = "insufficient_data",
        ProfileBuiltAt = DateTime.UtcNow
    };
}

// ── DTOs for Riot API responses ──────────────────────────────────────────
public class SummonerDto
{
    public string Puuid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}

public class RiotMatchDto
{
    public RiotMatchMetadata? Metadata { get; set; }
    public RiotMatchInfo? Info { get; set; }
}

public class RiotMatchMetadata
{
    public string MatchId { get; set; } = string.Empty;
}

public class RiotMatchInfo
{
    public long GameDuration { get; set; }
    public List<RiotParticipantDto>? Participants { get; set; }
    public List<RiotTeamDto>? Teams { get; set; }
}

public class RiotParticipantDto
{
    public string Puuid { get; set; } = string.Empty;
    public string SummonerName { get; set; } = string.Empty;
    public string ChampionName { get; set; } = string.Empty;
    public string TeamPosition { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public bool Win { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int TotalMinionsKilled { get; set; }
    public int NeutralMinionsKilled { get; set; }
    public double VisionScore { get; set; }
    public double TotalDamageDealtToChampions { get; set; }
    public int ObjectivesStolenAssists { get; set; }
    public RiotChallengeData? ChallengeData { get; set; }
}

public class RiotChallengeData
{
    public double GameScore { get; set; }
}

public class RiotTeamDto
{
    public int TeamId { get; set; }
    public RiotObjectivesDto? Objectives { get; set; }
}

public class RiotObjectivesDto
{
    public RiotObjectiveDto? All { get; set; }
}

public class RiotObjectiveDto
{
    public int Kills { get; set; }
}

// Internal match detail extracted per player
public class MatchDetail
{
    public string MatchId { get; set; } = string.Empty;
    public string Puuid { get; set; } = string.Empty;
    public string SummonerName { get; set; } = string.Empty;
    public bool Won { get; set; }
    public string Role { get; set; } = string.Empty;
    public string ChampionId { get; set; } = string.Empty;
    public double KDA { get; set; }
    public double CSPerMinute { get; set; }
    public double VisionScorePerMinute { get; set; }
    public double DamageShare { get; set; }
    public double AssistShare { get; set; }
    public double ObjectiveParticipation { get; set; }
    public double PerformanceScore { get; set; }
    public double ChampionEngageRating { get; set; }
    public double ChampionPeelRating { get; set; }
    public double ChampionPokeRating { get; set; }
    public double ChampionSplitRating { get; set; }
    public double ChampionTeamfightRating { get; set; }
}
