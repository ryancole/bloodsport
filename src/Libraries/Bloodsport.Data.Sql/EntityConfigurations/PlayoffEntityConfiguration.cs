using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class PlayoffEntityConfiguration : IEntityTypeConfiguration<Playoff>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Playoff> builder)
        {
            builder
                .Property(m => m.Status)
                .HasDefaultValue(PlayoffStatus.Upcoming);

            builder
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Season)
                .WithOne(m => m.Playoff)
                .IsRequired();
        }

        #endregion
    }
}
