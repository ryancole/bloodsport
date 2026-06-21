using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class PlayoffRound
    {
        #region Properties

        public long Id { get; private set; }

        public required long PlayoffId { get; set; }

        public required string Name { get; set; }

        public DateTime? DateEnd { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Nav

        [JsonIgnore]
        public virtual Playoff Playoff { get; set; }

        [JsonIgnore]
        public virtual ICollection<PlayoffRoundMatchup> PlayoffMatchups { get; set; } = [];

        #endregion
    }
}
