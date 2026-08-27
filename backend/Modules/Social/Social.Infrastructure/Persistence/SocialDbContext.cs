using Microsoft.EntityFrameworkCore;
using Social.Application;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialDbContext : DbContext, IUnitOfWork
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("social");
        builder.ApplyConfigurationsFromAssembly(typeof(SocialDbContext).Assembly);
    }
}
