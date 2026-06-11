using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class PlayoffMatchup
    {
        #region Properties

        public long Id { get; private set; }

        public required long SeasonId { get; set; }

        public required int Round { get; set; }

        public required int Position { get; set; }

        public long? TeamOneId { get; set; }

        public long? TeamTwoId { get; set; }

        public long? NextMatchupId { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual required Season Season { get; set; }

        [JsonIgnore]
        public virtual Team? TeamOne { get; set; }

        [JsonIgnore]
        public virtual Team? TeamTwo { get; set; }

        [JsonIgnore]
        public virtual PlayoffMatchup? NextMatchup { get; set; }

        [JsonIgnore]
        public virtual PlayoffMatchupResult? PlayoffMatchupResult { get; set; }

        #endregion
    }
}
