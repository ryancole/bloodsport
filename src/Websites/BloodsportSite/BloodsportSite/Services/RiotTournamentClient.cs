using Bloodsport.Entity.Database;
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

        public async Task<string> CreateTournamentCodeAsync(
            long tournamentId,
            SeasonMatchupParameters? parameters = null,
            string[]? allowedParticipants = null,
            string? metadata = null)
        {
            // Riot PUUIDs must be lowercase — normalize in case seeded/stored data is uppercase
            var normalizedParticipants = allowedParticipants?.Select(p => p.ToLowerInvariant()).ToArray();

            var teamSize = parameters?.TeamSize ?? 5;
            var pickType = parameters?.PickType ?? "TOURNAMENT_DRAFT";
            var mapType = parameters?.MapType ?? "SUMMONERS_RIFT";
            var spectatorType = parameters?.SpectatorType ?? "ALL";
            var enoughPlayers = parameters?.EnoughPlayers ?? false;

            string[] codes;

            if (RiotApiEndpoints.UseStub)
            {
                codes = await _api.TournamentStubV5().CreateTournamentCodeAsync(Route,
                    new StubNs.TournamentCodeParametersV5
                    {
                        TeamSize = teamSize,
                        PickType = pickType,
                        MapType = mapType,
                        SpectatorType = spectatorType,
                        EnoughPlayers = enoughPlayers,
                        Metadata = metadata,
                        AllowedParticipants = normalizedParticipants,
                    },
                    tournamentId, count: 1);
            }
            else
            {
                codes = await _api.TournamentV5().CreateTournamentCodeAsync(Route,
                    new TournNs.TournamentCodeParametersV5
                    {
                        TeamSize = teamSize,
                        PickType = pickType,
                        MapType = mapType,
                        SpectatorType = spectatorType,
                        EnoughPlayers = enoughPlayers,
                        Metadata = metadata,
                        AllowedParticipants = normalizedParticipants,
                    },
                    tournamentId, count: 1);
            }

            return codes?[0] ?? throw new InvalidOperationException("Riot API returned no tournament codes.");
        }
    }
}
