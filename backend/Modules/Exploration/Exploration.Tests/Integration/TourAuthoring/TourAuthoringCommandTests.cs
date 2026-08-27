using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringCommandTests : BaseIntegrationTest
{
    public TourAuthoringCommandTests(ExplorationApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_stores_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/exploration/tours",
            new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tour = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
        tour!.Id.Should().NotBeEmpty();
        tour.AuthorId.Should().Be(WellKnownUsers.Explorer);
        tour.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public async Task Create_rejects_a_blank_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/exploration/tours",
            new CreateTourDto("   ", "Opis nove ture.", TourDifficulty.Easy, ["planina"]));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_requires_authentication()
    {
        var response = await Client.PostAsJsonAsync("/api/exploration/tours",
            new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_requires_the_explorer_role()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync("/api/exploration/tours",
            new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddTransportTime_stores_the_time_on_the_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            new TransportTimeDto(TransportMode.Walking, 120));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var tours = await client.GetFromJsonAsync<List<TourDto>>("/api/exploration/tours/mine", JsonOptions);
        var tour = tours!.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        tour.TransportTimes.Should().ContainSingle(time => time.Transport == TransportMode.Walking && time.Minutes == 120);
    }

    [Fact]
    public async Task Publish_publishes_a_complete_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            new TransportTimeDto(TransportMode.Walking, 120));

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var published = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);
        var tour = published!.Items.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Publish_rejects_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
