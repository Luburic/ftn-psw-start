using Identity.Infrastructure;
using Identity.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<IdentityApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected readonly IdentityApiFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(IdentityApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<IdentityModuleDbContext>(IdentitySeed.All);
        Client = Factory.CreateClient();
    }
}
