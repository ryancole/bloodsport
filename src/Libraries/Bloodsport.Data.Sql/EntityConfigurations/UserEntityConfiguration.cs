using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .HasIndex(t => t.EntraObjectId)
                .IsUnique();

            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");
        }

        #endregion
    }
}
