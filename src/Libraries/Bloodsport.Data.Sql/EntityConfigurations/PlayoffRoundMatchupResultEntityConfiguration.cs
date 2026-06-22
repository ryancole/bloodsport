using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class PlayoffRoundMatchupResultEntityConfiguration : IEntityTypeConfiguration<PlayoffRoundMatchupResult>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<PlayoffRoundMatchupResult> builder)
        {
            builder
                .Property(r => r.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.PlayoffRoundMatchup)
                .WithMany(m => m.PlayoffRoundMatchupResults)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(m => m.WinningTeam)
                .WithMany(m => m.PlayoffRoundMatchupResults)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
