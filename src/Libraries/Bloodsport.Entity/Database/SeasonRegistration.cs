using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class SeasonRegistration
    {
        #region Properties

        public long Id { get; private set; }

        public long TeamId { get; set; }

        public long SeasonId { get; set; }

        public bool InauguralRegistration { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public required virtual Team Team { get; set; }

        [JsonIgnore]
        public required virtual Season Season { get; set; }

        #endregion
    }
}
