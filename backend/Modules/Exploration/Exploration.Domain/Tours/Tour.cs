using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class Tour : AggregateRoot
{
    private const int MinimumDescriptionLengthForPublishing = 100;

    private readonly List<TransportTime> _transportTimes = [];

    public Guid AuthorId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TourDifficulty Difficulty { get; private set; }
    public List<string> Tags { get; private set; }
    public TourStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public IReadOnlyList<TransportTime> TransportTimes => _transportTimes;

    private Tour()
    {
        Name = null!;
        Description = null!;
        Tags = null!;
    }

    public Tour(Guid authorId, string name, string description, TourDifficulty difficulty, List<string> tags)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A tour requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A tour requires a description.");
        }
        if (tags is null || tags.Count == 0)
        {
            throw new DomainException("A tour requires at least one tag.");
        }

        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Tags = tags;
        Status = TourStatus.Draft;
    }

    public void AddTransportTime(TransportMode transport, int minutes)
    {
        if (_transportTimes.Any(time => time.Transport == transport))
        {
            throw new DomainException($"The tour already has a time for {transport}.");
        }

        _transportTimes.Add(new TransportTime(transport, minutes));
    }

    public void Publish()
    {
        if (Status == TourStatus.Published)
        {
            throw new DomainException("The tour is already published.");
        }
        if (Description.Length < MinimumDescriptionLengthForPublishing)
        {
            throw new DomainException($"A tour can be published only with a description of at least {MinimumDescriptionLengthForPublishing} characters.");
        }
        if (_transportTimes.Count == 0)
        {
            throw new DomainException("A tour can be published only with at least one transport time.");
        }

        Status = TourStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }
}
