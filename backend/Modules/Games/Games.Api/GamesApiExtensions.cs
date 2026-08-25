using Microsoft.Extensions.DependencyInjection;

namespace Games.Api;

public static class GamesApiExtensions
{
    public static IMvcBuilder AddGamesControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(GamesApiExtensions).Assembly);
    }
}
