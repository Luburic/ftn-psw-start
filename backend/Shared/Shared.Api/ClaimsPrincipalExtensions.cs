using System.Security.Claims;

namespace Shared.Api;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return value == null
            ? throw new InvalidOperationException("The request has no authenticated user. Did you forget RequireAuthorization()?")
            : Guid.Parse(value);
    }
}
