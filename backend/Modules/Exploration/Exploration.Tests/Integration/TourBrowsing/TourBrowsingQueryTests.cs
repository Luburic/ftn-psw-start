using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPublished_returns_only_published_tours()
    {
        var author = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        await author.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            new TransportTimeDto(TransportMode.Walking, 120));
        await author.PostAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/publish", null);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPublished_pages_the_results()
    {
        var author = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        await author.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            new TransportTimeDto(TransportMode.Walking, 120));
        await author.PostAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/publish", null);
        var created = await author.PostAsJsonAsync("/api/exploration/tours",
            new CreateTourDto("Obilazak Dunavskog parka", "Obilazak počinje kod fontane u Dunavskom parku, nastavlja se pored jezera i starih platana, a završava se na Dunavskoj ulici.", TourDifficulty.Easy, ["priroda"]));
        var second = (await created.Content.ReadFromJsonAsync<TourDto>(JsonOptions))!;
        await author.PostAsJsonAsync($"/api/exploration/tours/{second.Id}/transport-times",
            new TransportTimeDto(TransportMode.Bicycle, 30));
        await author.PostAsync($"/api/exploration/tours/{second.Id}/publish", null);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(2);
    }
}
