using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class PlayoffMatchupResult
    {
        #region Properties

        public long Id { get; private set; }

        public required long PlayoffMatchupId { get; set; }

        public long? WinnerTeamId { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual required PlayoffMatchup PlayoffMatchup { get; set; }

        [JsonIgnore]
        public virtual Team? WinnerTeam { get; set; }

        #endregion
    }
}
