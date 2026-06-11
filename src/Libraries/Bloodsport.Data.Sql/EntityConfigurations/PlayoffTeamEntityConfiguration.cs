using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class PlayoffTeamEntityConfiguration : IEntityTypeConfiguration<PlayoffTeam>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<PlayoffTeam> builder)
        {
            builder
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Team)
                .WithMany(m => m.PlayoffTeams)
                .IsRequired();

            builder
                .HasOne(m => m.Playoff)
                .WithMany(m => m.PlayoffTeams)
                .IsRequired();
        }

        #endregion
    }
}
