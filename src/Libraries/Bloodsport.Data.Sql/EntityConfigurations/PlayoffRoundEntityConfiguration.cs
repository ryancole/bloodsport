using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class PlayoffRoundEntityConfiguration : IEntityTypeConfiguration<PlayoffRound>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<PlayoffRound> builder)
        {
            builder
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Playoff)
                .WithMany(m => m.PlayoffRounds)
                .IsRequired();
        }

        #endregion
    }
}
