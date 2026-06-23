using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class Post
    {
        #region Properties

        public long Id { get; protected set; }

        public required long UserId { get; set; }

        public required string Title { get; set; }

        public required string Markdown { get; set; }

        public DateTime DateCreated { get; protected set; }

        #endregion

        #region Nav Props

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual ICollection<PostComment> PostComments { get; set; } = [];

        #endregion
    }
}
