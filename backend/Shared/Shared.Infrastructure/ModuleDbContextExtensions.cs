using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure;

public static class ModuleDbContextExtensions
{
    public static IServiceCollection AddModuleDbContext<TContext>(
        this IServiceCollection services, IConfiguration configuration, string schema)
        where TContext : DbContext
    {
        return services.AddDbContext<TContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Database"),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema)));
    }
}
