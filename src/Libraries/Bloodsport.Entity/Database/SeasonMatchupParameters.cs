using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class SeasonMatchupParameters
    {
        #region Allowed Values

        // Legal values accepted by the Riot tournament code API.
        public static readonly string[] PickTypes = ["BLIND_PICK", "DRAFT_MODE", "ALL_RANDOM", "TOURNAMENT_DRAFT"];

        public static readonly string[] MapTypes = ["SUMMONERS_RIFT", "HOWLING_ABYSS"];

        public static readonly string[] SpectatorTypes = ["NONE", "LOBBYONLY", "ALL"];

        #endregion

        #region Properties

        public long Id { get; set; }

        public required long SeasonId { get; set; }

        public int TeamSize { get; set; } = 5;

        public string PickType { get; set; } = "TOURNAMENT_DRAFT";

        public string MapType { get; set; } = "SUMMONERS_RIFT";

        public string SpectatorType { get; set; } = "ALL";

        public DateTime DateCreated { get; set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual Season Season { get; set; }

        #endregion
    }
}
