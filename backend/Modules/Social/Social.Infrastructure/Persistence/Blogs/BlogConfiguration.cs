using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence.Blogs;

internal static class BlogConfiguration
{
    public static void ConfigureBlogs(this ModelBuilder modelBuilder)
    {
        ConfigureBlog(modelBuilder.Entity<Blog>());
        ConfigureComment(modelBuilder.Entity<Comment>());
    }

    private static void ConfigureBlog(EntityTypeBuilder<Blog> builder)
    {
        builder.Property(blog => blog.Title).HasMaxLength(200);
        builder.Property(blog => blog.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(blog => blog.Comments)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(blog => blog.Comments).AutoInclude();
    }

    private static void ConfigureComment(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.Property(comment => comment.Id).ValueGeneratedNever();
    }
}
