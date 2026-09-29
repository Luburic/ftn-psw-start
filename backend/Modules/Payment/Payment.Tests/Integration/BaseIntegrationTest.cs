using System.Text.Json;
using System.Text.Json.Serialization;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration;

public sealed class PaymentApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<PaymentApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly PaymentApiFactory Factory;

    protected BaseIntegrationTest(PaymentApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<PaymentDbContext>(PaymentSeed.All);
    }
}
