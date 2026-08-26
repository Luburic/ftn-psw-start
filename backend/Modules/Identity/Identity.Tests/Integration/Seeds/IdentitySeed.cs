namespace Identity.Tests.Integration.Seeds;

internal static class IdentitySeed
{
    public static object[] All => [.. RoleSeed.All, .. UserSeed.All];
}
