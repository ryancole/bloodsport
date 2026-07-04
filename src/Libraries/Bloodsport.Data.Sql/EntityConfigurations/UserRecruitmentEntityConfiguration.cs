using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class UserRecruitmentEntityConfiguration : IEntityTypeConfiguration<UserRecruitment>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<UserRecruitment> builder)
        {
            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.User)
                .WithOne(m => m.UserRecruitment);
        }

        #endregion
    }
}
