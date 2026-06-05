using System.Text.Json.Serialization;

namespace Bloodsport.Entity.Database
{
    public class User
    {
        #region Properties

        public long Id { get; private set; }

        public required string DisplayName { get; set; }

        public required string EntraObjectId { get; init; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        [JsonIgnore]
        public virtual ICollection<Team> ManagedTeams { get; private set; } = [];

        [JsonIgnore]
        public virtual ICollection<RiotAccount> RiotAccounts { get; private set; } = [];

        #endregion
    }
}
