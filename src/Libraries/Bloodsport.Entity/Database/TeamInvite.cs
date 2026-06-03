namespace Bloodsport.Entity.Database
{
    public class TeamInvite
    {
        #region Properties

        public long Id { get; private set; }

        public long TeamId { get; set; }

        public long RiotAccountId { get; set; }

        public TeamInviteStatus Status { get; set; } = TeamInviteStatus.Pending;

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        public required virtual Team Team { get; set; }

        public required virtual RiotAccount RiotAccount { get; set; }

        #endregion
    }
}
