using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Bloodsport.Entity.Database;

namespace Bloodsport.Data.Sql.EntityConfigurations
{
    internal class PostCommentEntityConfiguration : IEntityTypeConfiguration<PostComment>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<PostComment> builder)
        {
            builder
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .HasOne(m => m.Post)
                .WithMany(m => m.PostComments)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(m => m.User)
                .WithMany(m => m.PostComments)
                .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
