using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class RiotLobbyEventEntityConfiguration : IEntityTypeConfiguration<RiotLobbyEvent>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<RiotLobbyEvent> builder)
        {
            builder
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");
        }

        #endregion
    }
}
