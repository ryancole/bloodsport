using Bloodsport.Entity.RiotApi;
using Camille.Enums;
using Camille.RiotGames;
using StubNs = Camille.RiotGames.TournamentStubV5;
using TournNs = Camille.RiotGames.TournamentV5;

namespace BloodsportSite.Services
{
    public class RiotTournamentClient
    {
        private readonly RiotGamesApi _api;
        private readonly IConfiguration _config;

        public RiotTournamentClient(RiotGamesApi api, IConfiguration config)
        {
            _api = api;
            _config = config;
        }

        private RegionalRoute Route =>
            Enum.Parse<RegionalRoute>(_config["RiotApi:RegionalRoute"] ?? "AMERICAS", ignoreCase: true);

        public async Task<long> CreateProviderAsync(string callbackUrl)
        {
            var region = _config["RiotApi:Region"] ?? "NA";

            if (RiotApiEndpoints.UseStub)
                return await _api.TournamentStubV5().RegisterProviderDataAsync(Route,
                    new StubNs.ProviderRegistrationParametersV5 { Region = region, Url = callbackUrl });

            return await _api.TournamentV5().RegisterProviderDataAsync(Route,
                new TournNs.ProviderRegistrationParametersV5 { Region = region, Url = callbackUrl });
        }

        public async Task<long> CreateTournamentAsync(long providerId, string name)
        {
            if (RiotApiEndpoints.UseStub)
                return await _api.TournamentStubV5().RegisterTournamentAsync(Route,
                    new StubNs.TournamentRegistrationParametersV5 { ProviderId = (int)providerId, Name = name });

            return await _api.TournamentV5().RegisterTournamentAsync(Route,
                new TournNs.TournamentRegistrationParametersV5 { ProviderId = (int)providerId, Name = name });
        }

        public async Task<string> CreateTournamentCodeAsync(long tournamentId, int teamSize = 5)
        {
            string[] codes;

            if (RiotApiEndpoints.UseStub)
            {
                codes = await _api.TournamentStubV5().CreateTournamentCodeAsync(Route,
                    new StubNs.TournamentCodeParametersV5
                    {
                        TeamSize = teamSize,
                        PickType = "TOURNAMENT_DRAFT",
                        MapType = "SUMMONERS_RIFT",
                        SpectatorType = "ALL",
                        EnoughPlayers = false,
                    },
                    tournamentId, count: 1);
            }
            else
            {
                codes = await _api.TournamentV5().CreateTournamentCodeAsync(Route,
                    new TournNs.TournamentCodeParametersV5
                    {
                        TeamSize = teamSize,
                        PickType = "TOURNAMENT_DRAFT",
                        MapType = "SUMMONERS_RIFT",
                        SpectatorType = "ALL",
                        EnoughPlayers = false,
                    },
                    tournamentId, count: 1);
            }

            return codes?[0] ?? throw new InvalidOperationException("Riot API returned no tournament codes.");
        }
    }
}
