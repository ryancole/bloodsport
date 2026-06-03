namespace Bloodsport.Entity.Database
{
    public class Match
    {
        #region Properties

        public long Id { get; private set; }

        public long SeasonId { get; set; }

        public long HomeTeamId { get; set; }

        public long AwayTeamId { get; set; }

        public int WeekNumber { get; set; }

        public MatchStatus Status { get; set; } = MatchStatus.Pending;

        public long? WinnerTeamId { get; set; }

        public string? RiotMatchId { get; set; }

        public long? ReportedByUserId { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        public required virtual Season Season { get; set; }

        public required virtual Team HomeTeam { get; set; }

        public required virtual Team AwayTeam { get; set; }

        public virtual Team? WinnerTeam { get; set; }

        public virtual User? ReportedByUser { get; set; }

        #endregion
    }
}
