using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class TeamInvite
    {
        #region Properties

        public long Id { get; private set; }

        public required long TeamId { get; set; }

        public required long RiotAccountId { get; set; }

        public required TeamInviteType Type { get; set; } = TeamInviteType.Invite;

        public required TeamInviteStatus Status { get; set; } = TeamInviteStatus.Pending;

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public required virtual Team Team { get; set; }

        [JsonIgnore]
        public required virtual RiotAccount RiotAccount { get; set; }

        #endregion
    }

    public enum TeamInviteStatus
    {
        Pending = 0,
        Accepted = 1,
        Declined = 2,
    }

    public enum TeamInviteType
    {
        Invite,
        Application
    }
}
