using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class PlayoffMatchup
    {
        #region Properties

        public long Id { get; private set; }

        public required long PlayoffRoundId { get; set; }

        public required int MatchNumber { get; set; }

        public long? TeamOneId { get; set; }

        public long? TeamTwoId { get; set; }

        public long? WinningTeamId { get; set; }

        public long? NextMatchupId { get; set; }

        public string? TournamentCode { get; set; }

        public DateTime DateEnd { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual PlayoffRound PlayoffRound { get; set; }

        [JsonIgnore]
        public virtual PlayoffTeam? TeamOne { get; set; }

        [JsonIgnore]
        public virtual PlayoffTeam? TeamTwo { get; set; }

        [JsonIgnore]
        public virtual PlayoffTeam? WinningTeam { get; set; }

        [JsonIgnore]
        public virtual PlayoffMatchup? NextMatchup { get; set; }

        #endregion
    }
}
