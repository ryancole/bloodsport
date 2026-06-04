using Microsoft.EntityFrameworkCore;

using Bloodsport.Entity.Database;

using Bloodsport.Data.Sql.EntityConfigurations;

namespace Bloodsport.Data.Sql
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions options) : base(options)
        {

        }

        #region Methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new TeamEntityConfiguration().Configure(modelBuilder.Entity<Team>());
            new UserEntityConfiguration().Configure(modelBuilder.Entity<User>());
            new RiotAccountEntityConfiguration().Configure(modelBuilder.Entity<RiotAccount>());
            new TeamMembershipEntityConfiguration().Configure(modelBuilder.Entity<TeamMembership>());
            new TeamInviteEntityConfiguration().Configure(modelBuilder.Entity<TeamInvite>());
            new SeasonEntityConfiguration().Configure(modelBuilder.Entity<Season>());
            new SeasonRegistrationEntityConfiguration().Configure(modelBuilder.Entity<SeasonRegistration>());
        }

        #endregion

        #region Properties

        public DbSet<User> Users { get; set; }

        public DbSet<Team> Teams { get; set; }

        public DbSet<RiotAccount> RiotAccounts { get; set; }

        public DbSet<TeamMembership> TeamMemberships { get; set; }

        public DbSet<TeamInvite> TeamInvites { get; set; }

        public DbSet<Season> Seasons { get; set; }

        public DbSet<SeasonRegistration> SeasonRegistrations { get; set; }

        #endregion
    }
}
