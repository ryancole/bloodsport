using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class Season
    {
        #region Properties

        public long Id { get; private set; }

        public int Length { get; set; }

        public bool RegistrationOpen { get; set; }

        public long? RiotProviderId { get; set; }

        public long? RiotTournamentId { get; set; }

        public required string Name { get; set; }

        public required string RiotRegion { get; set; }

        public SeasonStatus Status { get; set; } = SeasonStatus.Upcoming;

        public DateTime? EstimatedDateEnd { get; set; }

        public DateTime? EstimatedDateStart { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual Playoff Playoff { get; set; }

        [JsonIgnore]
        public virtual SeasonMatchupParameters MatchupParameters { get; set; }

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

    public enum SeasonStatus
    {
        Upcoming = 0,
        PreSeason = 1,
        Active = 2,
        Completed = 4
    }
}
