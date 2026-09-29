using Games.Application;
using Games.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GamesDbContext>(configuration, "games");
        services.AddHostedService<GamesModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<GamesMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());

        return services;
    }
}
