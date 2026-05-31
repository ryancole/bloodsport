namespace Bloodsport.Api.Services;

// Wraps the Riot Tournament API v5.
// Requires a production API key with tournament access (not the default dev key).
// Request tournament access at: https://developer.riotgames.com/
public class RiotTournamentService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<RiotTournamentService> _logger;

    public RiotTournamentService(IHttpClientFactory factory, IConfiguration config, ILogger<RiotTournamentService> logger)
    {
        _http = factory.CreateClient("riot");
        _config = config;
        _logger = logger;
    }

    public async Task<int> RegisterProviderAsync(string callbackUrl, string region = "NA")
    {
        var payload = new { region, url = callbackUrl };
        var response = await _http.PostAsJsonAsync(
            "https://americas.api.riotgames.com/lol/tournament/v5/providers", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<int> CreateTournamentAsync(int providerId, string name)
    {
        var payload = new { providerId, name };
        var response = await _http.PostAsJsonAsync(
            "https://americas.api.riotgames.com/lol/tournament/v5/tournaments", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<List<string>> GenerateTournamentCodesAsync(
        int tournamentId,
        int count = 1,
        string mapType = "SUMMONERS_RIFT",
        string pickType = "TOURNAMENT_DRAFT",
        string spectatorType = "ALL",
        int teamSize = 5)
    {
        var payload = new { mapType, pickType, spectatorType, teamSize, metadata = string.Empty };
        var url = $"https://americas.api.riotgames.com/lol/tournament/v5/codes?count={count}&tournamentId={tournamentId}";
        var response = await _http.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<string>>() ?? new();
    }

    public async Task<RiotMatchResult?> GetMatchResultAsync(string matchId)
    {
        var response = await _http.GetAsync(
            $"https://americas.api.riotgames.com/lol/tournament/v5/games/by-code/{matchId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RiotMatchResult>();
    }
}

// Partial model of what Riot POSTs to your callback URL after a match
public class RiotMatchResult
{
    public string GameId { get; set; } = string.Empty;
    public string TournamentCode { get; set; } = string.Empty;
    public string WinningTeam { get; set; } = string.Empty;
    public List<RiotParticipant> Participants { get; set; } = new();
}

public class RiotParticipant
{
    public string SummonerName { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public bool Win { get; set; }
}
