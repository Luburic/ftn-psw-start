using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Identity.Tests.Integration;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connection = Environment.GetEnvironmentVariable("EXPLORER_TEST_DATABASE")
            ?? "Host=localhost;Port=5432;Database=explorer-test;Username=postgres;Password=admin";

        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Database", connection);
    }
}
