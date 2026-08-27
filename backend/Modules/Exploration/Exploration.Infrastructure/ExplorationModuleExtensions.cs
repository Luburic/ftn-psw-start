using Exploration.Application;
using Exploration.Application.TourAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Infrastructure.Persistence.Tours;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Exploration.Infrastructure;

public static class ExplorationModuleExtensions
{
    public static IServiceCollection AddExplorationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ExplorationDbContext>(configuration, "exploration");
        services.AddHostedService<ExplorationModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<ExplorationMapperProfile>());

        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ExplorationDbContext>());
        services.AddScoped<ITourReadRepository, TourReadRepository>();

        services.AddScoped<TourAuthoringService>();
        services.AddScoped<TourAuthoringQueries>();
        services.AddScoped<TourBrowsingQueries>();

        return services;
    }
}
