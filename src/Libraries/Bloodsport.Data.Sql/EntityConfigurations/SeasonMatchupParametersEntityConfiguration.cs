using Bloodsport.Entity.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class SeasonMatchupParametersEntityConfiguration : IEntityTypeConfiguration<SeasonMatchupParameters>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<SeasonMatchupParameters> builder)
        {
            builder
                .Property(r => r.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Season)
                .WithOne(m => m.MatchupParameters);
        }

        #endregion
    }
}
