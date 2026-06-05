using System;
using System.Collections.Generic;
using System.Text;
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
        public virtual SeasonWeek SeasonWeek { get; set; }

        [JsonIgnore]
        public virtual Team TeamOne { get; set; }

        [JsonIgnore]
        public virtual Team TeamTwo { get; set; }

        #endregion
    }
}
