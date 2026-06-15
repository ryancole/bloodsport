using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class TeamPlayoffRosterEntityConfiguration : IEntityTypeConfiguration<TeamPlayoffRoster>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<TeamPlayoffRoster> builder)
        {
            builder
                .HasIndex(m => new { m.TeamId, m.PlayoffId })
                .IsUnique();

            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Team)
                .WithMany(m => m.TeamPlayoffRosters)
                .IsRequired();

            builder
                .HasOne(m => m.Playoff)
                .WithMany(m => m.TeamPlayoffRosters)
                .IsRequired();
        }

        #endregion
    }
}
