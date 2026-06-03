using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class RiotAccountEntityConfiguration : IEntityTypeConfiguration<RiotAccount>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<RiotAccount> builder)
        {
            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .Property(t => t.Puuid)
                .HasMaxLength(78);

            builder
                .Property(t => t.GameName)
                .HasMaxLength(16);

            builder
                .Property(t => t.TagLine)
                .HasMaxLength(5);

            builder
                .HasIndex(t => t.Puuid)
                .IsUnique();

            builder
                .HasOne(m => m.User)
                .WithMany(m => m.RiotAccounts)
                .IsRequired();
        }

        #endregion
    }
}
