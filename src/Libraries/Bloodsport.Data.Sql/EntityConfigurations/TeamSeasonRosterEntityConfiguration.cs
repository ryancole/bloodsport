using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class TeamSeasonRosterEntityConfiguration : IEntityTypeConfiguration<TeamSeasonRoster>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<TeamSeasonRoster> builder)
        {
            builder
                .HasIndex(m => new { m.TeamId, m.SeasonId })
                .IsUnique();

            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Team)
                .WithMany(m => m.TeamSeasonRosters)
                .IsRequired();

            builder
                .HasOne(m => m.Season)
                .WithMany(m => m.TeamSeasonRosters)
                .IsRequired();
        }

        #endregion
    }
}
