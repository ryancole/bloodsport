using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class TeamInviteEntityConfiguration : IEntityTypeConfiguration<TeamInvite>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<TeamInvite> builder)
        {
            builder
                .Property(m => m.Type)
                .IsRequired()
                .HasDefaultValue(TeamInviteType.Invite);

            builder
                .Property(m => m.Status)
                .IsRequired()
                .HasDefaultValue(TeamInviteStatus.Pending);

            builder
                .Property(t => t.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(i => i.Team)
                .WithMany(t => t.TeamInvites)
                .IsRequired();

            builder
                .HasOne(i => i.RiotAccount)
                .WithMany(r => r.TeamInvites)
                .IsRequired();
        }

        #endregion
    }
}
