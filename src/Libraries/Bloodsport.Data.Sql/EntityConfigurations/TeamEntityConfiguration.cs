using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class TeamEntityConfiguration : IEntityTypeConfiguration<Team>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder
                .Property(m => m.Name)
                .IsRequired();

            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Manager)
                .WithMany(m => m.ManagedTeams)
                .HasForeignKey(m => m.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
