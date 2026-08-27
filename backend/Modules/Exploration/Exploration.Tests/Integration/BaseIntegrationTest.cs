using System.Text.Json;
using System.Text.Json.Serialization;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration;

public sealed class ExplorationApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<ExplorationApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly ExplorationApiFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(ExplorationApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<ExplorationDbContext>(ExplorationSeed.All);
        Client = Factory.CreateClient();
    }
}
