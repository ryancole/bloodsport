using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class MatchEntityConfiguration : IEntityTypeConfiguration<Match>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder
                .Property(m => m.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Season)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(m => m.WinnerTeam)
                .WithMany()
                .HasForeignKey(m => m.WinnerTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(m => m.ReportedByUser)
                .WithMany()
                .HasForeignKey(m => m.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
