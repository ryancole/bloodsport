using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

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
                .HasOne(m => m.PlayoffRound)
                .WithMany(m => m.PlayoffMatchups);

            builder
                .HasOne(m => m.TeamOne)
                .WithMany()
                .HasForeignKey(m => m.TeamOneId);

            builder
                .HasOne(m => m.TeamTwo)
                .WithMany()
                .HasForeignKey(m => m.TeamTwoId);

            builder
                .HasOne(m => m.NextMatchup)
                .WithMany()
                .HasForeignKey(m => m.NextMatchupId);
        }

        #endregion
    }
}
