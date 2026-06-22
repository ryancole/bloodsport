using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class PlayoffTeam
    {
        #region Properties

        public long Id { get; private set; }

        public required long TeamId { get; set; }

        public required long PlayoffId { get; set; }

        public required int Seed { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Nav

        [JsonIgnore]
        public virtual Team Team { get; set; }

        [JsonIgnore]
        public virtual Playoff Playoff { get; set; }

        [JsonIgnore]
        public virtual ICollection<PlayoffRoundMatchupResult> PlayoffRoundMatchupResults { get; set; } = [];

        #endregion
    }
}
