using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class SeasonWeekMatchupResultEntityConfiguration : IEntityTypeConfiguration<SeasonWeekMatchupResult>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<SeasonWeekMatchupResult> builder)
        {
            builder
                .Property(r => r.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(r => r.SeasonWeekMatchup)
                .WithOne(m => m.SeasonWeekMatchupResult)
                .HasForeignKey<SeasonWeekMatchupResult>(r => r.SeasonWeekMatchupId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder
                .HasOne(r => r.WinnerTeam)
                .WithMany(r => r.SeasonWeekMatchupResults)
                .HasForeignKey(r => r.WinnerTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
