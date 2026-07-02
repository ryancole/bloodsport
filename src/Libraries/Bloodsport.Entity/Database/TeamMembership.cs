using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class TeamMembership
    {
        /// <summary>Maximum number of members a team may have marked <see cref="Active"/> at once (a full season roster).</summary>
        public const int MaxActiveMembers = 7;

        /// <summary>Minimum number of active members a team needs to field a lineup and participate in a season.</summary>
        public const int MinActiveMembers = 5;

        #region Properties

        public long Id { get; private set; }

        public required long TeamId { get; set; }

        public required long RiotAccountId { get; set; }

        public required bool Active { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public required virtual Team Team { get; set; }

        [JsonIgnore]
        public required virtual RiotAccount RiotAccount { get; set; }

        #endregion
    }
}
