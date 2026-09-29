using System.Text.Json;
using System.Text.Json.Serialization;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration;

public sealed class GamesApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<GamesApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly GamesApiFactory Factory;

    protected BaseIntegrationTest(GamesApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<GamesDbContext>(GamesSeed.All);
    }
}
