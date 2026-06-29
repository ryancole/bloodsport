using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class SeasonMatchupParameters
    {
        #region Properties

        public long Id { get; set; }

        public required long SeasonId { get; set; }

        public DateTime DateCreated { get; set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual Season Season { get; set; }

        #endregion
    }
}
