using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class RiotAccountRecruitment
    {
        #region Properties

        public long Id { get; private set; }

        public required long RiotAccountId { get; set; }

        public bool IsLookingForTeam { get; set; }

        public ICollection<RiotAccountRecruitmentLanes> Lanes { get; set; } = [];

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual RiotAccount RiotAccount { get; set; }

        #endregion
    }

    public enum RiotAccountRecruitmentLanes
    {
        #region Values

        Top,
        Middle,
        Jungle,
        Bottom,
        Support

        #endregion
    }
}
