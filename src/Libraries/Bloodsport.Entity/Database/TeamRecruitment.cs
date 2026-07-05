using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class TeamRecruitment
    {
        #region Properties

        public long Id { get; private set; }

        public required long TeamId { get; set; }

        public bool IsLookingForUser { get; set; }

        public ICollection<TeamRecruitmentLanes> Lanes { get; set; } = [];

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual Team Team { get; set; }

        #endregion
    }

    public enum TeamRecruitmentLanes
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
