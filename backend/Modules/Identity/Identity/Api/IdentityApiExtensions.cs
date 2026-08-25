using Microsoft.Extensions.DependencyInjection;

namespace Identity.Api;

public static class IdentityApiExtensions
{
    public static IMvcBuilder AddIdentityControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(IdentityApiExtensions).Assembly);
    }
}
