using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class TeamMembershipEntityConfiguration : IEntityTypeConfiguration<TeamMembership>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<TeamMembership> builder)
        {
            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Team)
                .WithMany(m => m.TeamMemberships)
                .IsRequired();

            builder
                .HasOne(m => m.RiotAccount)
                .WithMany(m => m.TeamMemberships)
                .IsRequired();
        }

        #endregion
    }
}
