using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence.Blogs;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.Property(comment => comment.Id).ValueGeneratedNever();
    }
}
