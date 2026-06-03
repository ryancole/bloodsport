namespace Bloodsport.Entity.Database
{
    public class Team
    {
        #region Properties

        public long Id { get; private set; }

        public long ManagerId { get; set; }

        public required string Name { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        public required virtual User Manager { get; set; }

        public virtual ICollection<TeamMembership> TeamMemberships { get; set; } = [];

        public virtual ICollection<TeamInvite> TeamInvites { get; set; } = [];

        #endregion
    }
}
