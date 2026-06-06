using System.Text.Json.Serialization;

namespace Bloodsport.Entity.RiotApi
{
    public class RiotTournamentCallbackPayload
    {
        [JsonPropertyName("shortCode")]
        public string ShortCode { get; set; } = string.Empty;

        [JsonPropertyName("participants")]
        public List<RiotTournamentParticipant> Participants { get; set; } = [];
    }

    public class RiotTournamentParticipant
    {
        [JsonPropertyName("puuid")]
        public string Puuid { get; set; } = string.Empty;

        [JsonPropertyName("team")]
        public int Team { get; set; }

        [JsonPropertyName("win")]
        public bool Win { get; set; }
    }
}
