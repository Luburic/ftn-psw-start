using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourTests
{
    private static readonly string LongDescription = new('o', 100);

    private static Tour CreateTour(string description) =>
        new(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"]);

    [Fact]
    public void Creation_produces_a_draft()
    {
        var tour = CreateTour(LongDescription);

        tour.Status.Should().Be(TourStatus.Draft);
        tour.PublishedAt.Should().BeNull();
        tour.TransportTimes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_name(string name)
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, name, "Opis ture.", TourDifficulty.Easy, ["istorija"]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_description(string description)
    {
        var creation = () => CreateTour(description);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_rejects_empty_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddTransportTime_rejects_a_duplicate_transport()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, 90);

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void AddTransportTime_rejects_non_positive_minutes(int minutes)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, minutes);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Publish_publishes_a_complete_tour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Bicycle, 45);

        tour.Publish();

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Publish_rejects_a_short_description()
    {
        var tour = CreateTour("Kratak opis.");
        tour.AddTransportTime(TransportMode.Walking, 120);

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Publish_requires_a_transport_time()
    {
        var tour = CreateTour(LongDescription);

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Publish_rejects_an_already_published_tour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Car, 30);
        tour.Publish();

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }
}
