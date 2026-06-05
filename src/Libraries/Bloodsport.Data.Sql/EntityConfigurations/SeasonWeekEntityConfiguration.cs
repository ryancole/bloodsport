using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class SeasonWeekEntityConfiguration : IEntityTypeConfiguration<SeasonWeek>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<SeasonWeek> builder)
        {
            builder
                .Property(r => r.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Season)
                .WithMany(m => m.SeasonWeeks)
                .IsRequired();
        }

        #endregion
    }
}
