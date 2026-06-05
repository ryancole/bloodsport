using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class SeasonWeekMatchupEntityConfiguration : IEntityTypeConfiguration<SeasonWeekMatchup>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<SeasonWeekMatchup> builder)
        {
            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.TeamOne)
                .WithMany()
                .HasForeignKey(m => m.TeamOneId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasOne(m => m.TeamTwo)
                .WithMany()
                .HasForeignKey(m => m.TeamTwoId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasOne(m => m.SeasonWeek)
                .WithMany(m => m.SeasonWeekMatchups)
                .IsRequired();
        }

        #endregion
    }
}
