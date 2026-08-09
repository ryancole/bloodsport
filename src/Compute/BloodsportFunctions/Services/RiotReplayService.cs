using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Camille.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Services;

/// <summary>
/// Wraps match-v5's replay endpoint. Camille does not generate a binding for it, so this
/// talks to the regional host directly.
/// </summary>
public class RiotReplayService
{
    private const string ApiKeyHeader = "X-Riot-Token";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<RiotReplayService> _logger;

    public RiotReplayService(HttpClient httpClient, IConfiguration config, ILogger<RiotReplayService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    private string ApiKey =>
        _config["RiotApi:ApiKey"] ?? throw new InvalidOperationException("RiotApi:ApiKey is not configured.");

    /// <summary>
    /// Lists the .rofl download URLs Riot currently holds for a player. The list is per-player
    /// rather than per-match, so callers filter it down to the game they want.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetReplayUrlsAsync(
        RegionalRoute route,
        string puuid,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://{route.ToString().ToLowerInvariant()}.api.riotgames.com" +
                  $"/lol/match/v5/matches/by-puuid/{puuid}/replays";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add(ApiKeyHeader, ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return [];

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ReplayResponse>(cancellationToken);

        return payload?.MatchFileUrls ?? [];
    }

    /// <summary>
    /// Fetches a .rofl file. The response is returned with headers only, so the caller streams
    /// the body and is responsible for disposing it.
    /// </summary>
    public async Task<HttpResponseMessage> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var response = await SendDownloadAsync(fileUrl, withApiKey: false, cancellationToken);

        // Unverified whether the URLs are pre-signed or expect the API key. Assume pre-signed and
        // fall back to the key so a change on Riot's side does not silently break the pipeline.
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            _logger.LogInformation(
                "Replay download returned {StatusCode} without credentials; retrying with the API key.",
                (int)response.StatusCode);

            response.Dispose();
            response = await SendDownloadAsync(fileUrl, withApiKey: true, cancellationToken);
        }

        response.EnsureSuccessStatusCode();

        return response;
    }

    private Task<HttpResponseMessage> SendDownloadAsync(string fileUrl, bool withApiKey, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, fileUrl);

        if (withApiKey)
            request.Headers.Add(ApiKeyHeader, ApiKey);

        return _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private sealed class ReplayResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("matchFileURLs")]
        public string[] MatchFileUrls { get; set; } = [];
    }
}
