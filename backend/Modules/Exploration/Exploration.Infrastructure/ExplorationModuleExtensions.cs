using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Exploration.Infrastructure;

public static class ExplorationModuleExtensions
{
    public static IServiceCollection AddExplorationModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
