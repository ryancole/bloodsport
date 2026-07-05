using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class RiotAccountRecruitmentEntityConfiguration : IEntityTypeConfiguration<RiotAccountRecruitment>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<RiotAccountRecruitment> builder)
        {
            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.RiotAccount)
                .WithOne(m => m.RiotAccountRecruitment);
        }

        #endregion
    }
}
