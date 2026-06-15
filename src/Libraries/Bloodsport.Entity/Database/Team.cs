using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class Team
    {
        #region Properties

        public long Id { get; private set; }

        public long ManagerId { get; set; }

        public required string Name { get; set; }

        public string? LogoUrl { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public required virtual User Manager { get; set; }

        [JsonIgnore]
        public virtual ICollection<TeamInvite> TeamInvites { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<TeamMembership> TeamMemberships { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<SeasonRegistration> SeasonRegistrations { get; set;} = [];

        [JsonIgnore]
        public virtual ICollection<SeasonWeekMatchupResult> SeasonWeekMatchupResults { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<TeamSeasonResult> TeamSeasonResults { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<TeamSeasonRoster> TeamSeasonRosters { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<PlayoffTeam> PlayoffTeams { get; set;  } = [];

        [JsonIgnore]
        public virtual ICollection<TeamPlayoffRoster> TeamPlayoffRosters { get; set; } = [];

        #endregion
    }
}
