using Microsoft.AspNetCore.Identity;

namespace Identity.Tests.Integration.Seeds;

internal static class RoleSeed
{
    public static readonly IdentityRole<Guid> Administrator = new() { Id = Guid.Parse("00000000-0000-0000-0001-000000000001"), Name = "administrator", NormalizedName = "ADMINISTRATOR" };
    public static readonly IdentityRole<Guid> Explorer = new() { Id = Guid.Parse("00000000-0000-0000-0001-000000000002"), Name = "explorer", NormalizedName = "EXPLORER" };

    public static object[] All => [Administrator, Explorer];
}
