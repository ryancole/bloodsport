using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class TeamSeasonRoster
    {
        #region Properties

        public long Id { get; private set; }

        public required long TeamId { get; set; }

        public required long SeasonId { get; set; }

        public required string RosterJson { get; set;  }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Nav

        [JsonIgnore]
        public virtual Team Team { get; set;  }

        [JsonIgnore]
        public virtual Season Season { get; set;  }

        #endregion
    }

    public class TeamSeasonRosterJson
    {
        #region Properties

        public ICollection<string> AllowedSummonerNames { get; set; } = [];

        #endregion
    }
}
