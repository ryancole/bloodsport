using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class RiotAccount
    {
        #region Properties

        public long Id { get; private set; }

        public long UserId { get; set; }

        public required string Puuid { get; set; }

        public required string GameName { get; set; }

        public required string TagLine { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual required User User { get; set; }

        [JsonIgnore]
        public virtual ICollection<TeamMembership> TeamMemberships { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<TeamInvite> TeamInvites { get; set; } = [];

        [JsonIgnore]
        public virtual RiotAccountRecruitment RiotAccountRecruitment { get; set; }

        #endregion
    }
}
