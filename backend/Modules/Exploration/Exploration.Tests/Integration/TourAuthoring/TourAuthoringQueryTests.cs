using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringQueryTests : BaseIntegrationTest
{
    public TourAuthoringQueryTests(ExplorationApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetMine_returns_only_the_callers_tours()
    {
        var otherAuthor = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        await otherAuthor.PostAsJsonAsync("/api/exploration/tours",
            new CreateTourDto("Tuđa tura", "Opis tuđe ture.", TourDifficulty.Easy, ["grad"]));
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<TourDto>>("/api/exploration/tours/mine", JsonOptions);

        tours.Should().HaveCount(2);
        tours.Should().OnlyContain(tour => tour.AuthorId == WellKnownUsers.Explorer);
    }
}
