using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class PlayoffRoundMatchupResult
    {
        #region Properties

        public long Id { get; private set; }

        public required long PlayoffRoundMatchupId { get; set; }

        public long? WinningTeamId { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Nav props

        [JsonIgnore]
        public virtual PlayoffTeam WinningTeam { get; set; }

        [JsonIgnore]
        public virtual PlayoffRoundMatchup PlayoffRoundMatchup { get; set; }

        #endregion
    }
}
