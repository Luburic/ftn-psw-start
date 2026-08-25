using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
