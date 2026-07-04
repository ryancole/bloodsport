using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class UserRecruitment
    {
        #region Properties

        public long Id { get; private set; }

        public required long UserId { get; set; }

        public bool IsLookingForTeam { get; set; }

        public ICollection<UserRecruitmentLanes> Lanes { get; set; } = [];

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual User User { get; set; }

        #endregion
    }

    public enum UserRecruitmentLanes
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
