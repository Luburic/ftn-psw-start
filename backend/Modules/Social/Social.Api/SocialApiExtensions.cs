using Microsoft.Extensions.DependencyInjection;

namespace Social.Api;

public static class SocialApiExtensions
{
    public static IMvcBuilder AddSocialControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(SocialApiExtensions).Assembly);
    }
}
