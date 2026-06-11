using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class PlayoffMatchupEntityConfiguration : IEntityTypeConfiguration<PlayoffMatchup>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<PlayoffMatchup> builder)
        {
            builder
                .Property(m => m.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Season)
                .WithMany(s => s.PlayoffMatchups)
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder
                .HasOne(m => m.TeamOne)
                .WithMany()
                .HasForeignKey(m => m.TeamOneId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(m => m.TeamTwo)
                .WithMany()
                .HasForeignKey(m => m.TeamTwoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(m => m.NextMatchup)
                .WithMany()
                .HasForeignKey(m => m.NextMatchupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(m => m.PlayoffMatchupResult)
                .WithOne(r => r.PlayoffMatchup)
                .HasForeignKey<PlayoffMatchupResult>(r => r.PlayoffMatchupId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
