using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class Playoff
    {
        #region Properties

        public long Id { get; private set; }

        public required long SeasonId { get; set; }

        public required string Name { get; set; }

        public long? RiotProviderId { get; set; }

        public long? RiotTournamentId { get; set; }

        public required PlayoffStatus Status { get; set; } = PlayoffStatus.Upcoming;

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Nav

        [JsonIgnore]
        public virtual Season Season { get; set; }

        [JsonIgnore]
        public virtual ICollection<PlayoffTeam> PlayoffTeams { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<PlayoffRound> PlayoffRounds { get; set; } = [];

        #endregion
    }

    public enum PlayoffStatus
    {
        Upcoming,
        Active,
        Completed
    }
}
