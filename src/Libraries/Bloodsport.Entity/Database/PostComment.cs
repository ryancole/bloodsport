using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class PostComment
    {
        #region Properties

        public long Id { get; protected set; }

        public required long PostId { get; set; }

        public required long UserId { get; set; }

        public required string Body { get; set; }

        public DateTime DateCreated { get; protected set; }

        #endregion

        #region Nav Props

        [JsonIgnore]
        public virtual Post Post { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        #endregion
    }
}
