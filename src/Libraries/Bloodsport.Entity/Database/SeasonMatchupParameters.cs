using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class SeasonMatchupParameters
    {
        #region Properties

        public long Id { get; set; }

        public required long SeasonId { get; set; }

        public int TeamSize { get; set; } = 5;

        public string PickType { get; set; } = "TOURNAMENT_DRAFT";

        public string MapType { get; set; } = "SUMMONERS_RIFT";

        public string SpectatorType { get; set; } = "ALL";

        public bool EnoughPlayers { get; set; }

        public string? Metadata { get; set; }

        public DateTime DateCreated { get; set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual Season Season { get; set; }

        #endregion
    }
}
