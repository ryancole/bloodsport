using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class SeasonRegistrationEntityConfiguration : IEntityTypeConfiguration<SeasonRegistration>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<SeasonRegistration> builder)
        {
            builder
                .HasIndex(r => new { r.SeasonId, r.TeamId })
                .IsUnique();

            builder
                .Property(r => r.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(r => r.Season)
                .WithMany(s => s.Registrations)
                .HasForeignKey(r => r.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(r => r.Team)
                .WithMany()
                .HasForeignKey(r => r.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
