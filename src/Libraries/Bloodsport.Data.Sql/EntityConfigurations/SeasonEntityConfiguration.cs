using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class SeasonEntityConfiguration : IEntityTypeConfiguration<Season>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Season> builder)
        {
            builder
                .Property(s => s.Name)
                .IsRequired();

            builder
                .Property(m => m.RegistrationOpen)
                .HasDefaultValue(false);

            builder
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");
        }

        #endregion
    }
}
