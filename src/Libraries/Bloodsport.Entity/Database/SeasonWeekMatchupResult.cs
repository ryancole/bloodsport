using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class SeasonWeekMatchupResult
    {
        #region Properties

        public long Id { get; private set; }

        public bool RosterMismatch { get; set; }

        public required long WinnerTeamId { get; set; }

        public required long SeasonWeekMatchupId { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Nav props

        [JsonIgnore]
        public virtual required Team WinnerTeam { get; set; }

        [JsonIgnore]
        public virtual required SeasonWeekMatchup SeasonWeekMatchup { get; set; }

        #endregion
    }
}
