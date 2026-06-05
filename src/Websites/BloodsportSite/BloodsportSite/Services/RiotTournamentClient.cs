using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BloodsportSite.Services
{
    public class RiotTournamentClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public RiotTournamentClient(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _apiKey = configuration["RiotApi:ApiKey"]
                ?? throw new InvalidOperationException("RiotApi:ApiKey is not configured.");

            _http.BaseAddress = new Uri(
                configuration["RiotApi:BaseUrl"]
                ?? throw new InvalidOperationException("RiotApi:BaseUrl is not configured."));
        }

        public async Task<long> CreateProviderAsync(string region, string callbackUrl)
        {
            var body = new { region, url = callbackUrl };
            var response = await PostAsync("/lol/tournament/v5/providers", body);
            return JsonSerializer.Deserialize<long>(await response.Content.ReadAsStringAsync());
        }

        public async Task<long> CreateTournamentAsync(long providerId, string name)
        {
            var body = new { providerId, name };
            var response = await PostAsync("/lol/tournament/v5/tournaments", body);
            return JsonSerializer.Deserialize<long>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string> CreateTournamentCodeAsync(long tournamentId, long teamSize = 5)
        {
            var body = new
            {
                teamSize,
                pickType = "TOURNAMENT_DRAFT",
                mapType = "SUMMONERS_RIFT",
                spectatorType = "ALL",
                enoughPlayers = false,
            };

            var response = await PostAsync(
                $"/lol/tournament/v5/codes?tournamentId={tournamentId}&count=1",
                body);

            var codes = JsonSerializer.Deserialize<string[]>(await response.Content.ReadAsStringAsync(), _json);
            return codes?[0] ?? throw new InvalidOperationException("Riot API returned no tournament codes.");
        }

        private async Task<HttpResponseMessage> PostAsync(string path, object body)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Add("X-Riot-Token", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, _json),
                Encoding.UTF8,
                "application/json");

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
