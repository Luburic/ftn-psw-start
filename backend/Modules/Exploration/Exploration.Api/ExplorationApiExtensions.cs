using Microsoft.Extensions.DependencyInjection;

namespace Exploration.Api;

public static class ExplorationApiExtensions
{
    public static IMvcBuilder AddExplorationControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(ExplorationApiExtensions).Assembly);
    }
}
