using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Social.Infrastructure;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
