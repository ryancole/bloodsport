using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class PostEntityConfiguration : IEntityTypeConfiguration<Post>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder
                .Property(p => p.Title)
                .IsRequired();

            builder
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.User)
                .WithMany(m => m.Posts);
        }

        #endregion
    }
}
