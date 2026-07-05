using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class TeamRecruitmentEntityConfiguration : IEntityTypeConfiguration<TeamRecruitment>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<TeamRecruitment> builder)
        {
            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Team)
                .WithOne(m => m.TeamRecruitment);
        }

        #endregion
    }
}
