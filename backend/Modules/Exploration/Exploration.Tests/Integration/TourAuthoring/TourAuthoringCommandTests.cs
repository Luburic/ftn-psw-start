using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringCommandTests : BaseIntegrationTest
{
    public TourAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Create_stores_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"]);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore + 1);
        var stored = assertContext.Tours.Single(tour => tour.Id == created.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.TransportTimes.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_rejects_a_blank_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("   ", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore);
    }

    [Fact]
    public async Task Create_requires_authentication()
    {
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);

        var response = await Client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_requires_the_explorer_role()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddTransportTime_stores_the_time_on_the_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TransportTimeDto(TransportMode.Walking, 120);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.TransportTimes.Should().ContainSingle(time => time.Transport == TransportMode.Walking && time.Minutes == 120);
    }

    [Fact]
    public async Task Publish_publishes_a_complete_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Publish_rejects_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }
}
