using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class PlayoffMatchupResultEntityConfiguration : IEntityTypeConfiguration<PlayoffMatchupResult>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<PlayoffMatchupResult> builder)
        {
            builder
                .Property(r => r.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(r => r.WinnerTeam)
                .WithMany()
                .HasForeignKey(r => r.WinnerTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
