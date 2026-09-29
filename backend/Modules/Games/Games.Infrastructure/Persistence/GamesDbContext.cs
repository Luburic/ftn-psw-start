using Games.Application;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence;

internal sealed class GamesDbContext : DbContext, IUnitOfWork
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("games");
    }
}
