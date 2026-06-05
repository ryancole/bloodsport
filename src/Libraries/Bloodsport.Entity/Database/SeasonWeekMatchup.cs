using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class SeasonWeekMatchup
    {
        #region Properties

        public long Id { get; private set; }

        public required long SeasonWeekId { get; set; }

        public required long TeamOneId { get; set; }

        public required long TeamTwoId { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Nav props

        [JsonIgnore]
        public virtual required Team TeamOne { get; set; }

        [JsonIgnore]
        public virtual required Team TeamTwo { get; set; }

        [JsonIgnore]
        public virtual required SeasonWeek SeasonWeek { get; set; }

        #endregion
    }
}
