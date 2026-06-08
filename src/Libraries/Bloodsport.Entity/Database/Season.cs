using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class Season
    {
        #region Properties

        public long Id { get; private set; }

        public bool RegistrationOpen { get; set; }

        public long? RiotProviderId { get; set; }

        public long? RiotTournamentId { get; set; }

        public required string Name { get; set; }

        public SeasonStatus Status { get; set; } = SeasonStatus.Upcoming;

        public DateTime? EstimatedDateEnd { get; set; }

        public DateTime? EstimatedDateStart { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual ICollection<SeasonWeek> SeasonWeeks { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<SeasonRegistration> SeasonRegistrations { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<TeamSeasonResult> TeamSeasonResults { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<TeamSeasonRoster> TeamSeasonRosters { get; set; } = [];

        #endregion
    }
}
