using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class SeasonWeek
    {
        #region Properties

        public long Id { get; set; }

        public required long SeasonId { get; set; }  

        public required int Index { get; set; }

        public required string Name { get; set;  }

        public DateTime DateEnd { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateCreated { get; set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public required virtual Season Season { get; set; }

        [JsonIgnore]
        public virtual ICollection<SeasonWeekMatchup> SeasonWeekMatchups { get; set; } = [];

        #endregion
    }
}
