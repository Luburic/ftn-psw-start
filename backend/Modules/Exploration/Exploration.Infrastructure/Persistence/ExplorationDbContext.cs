using Exploration.Application;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationDbContext : DbContext, IUnitOfWork
{
    public ExplorationDbContext(DbContextOptions<ExplorationDbContext> options) : base(options)
    {
    }

    public DbSet<Tour> Tours => Set<Tour>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("exploration");
        builder.ApplyConfigurationsFromAssembly(typeof(ExplorationDbContext).Assembly);
    }
}
