using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence.Blogs;

internal sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.Property(blog => blog.Title).HasMaxLength(200);
        builder.Property(blog => blog.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(blog => blog.Comments)
            .WithOne()
            .HasForeignKey("BlogId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(blog => blog.Comments).AutoInclude();
    }
}
