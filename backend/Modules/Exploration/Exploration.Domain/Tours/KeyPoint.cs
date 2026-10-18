using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class KeyPoint : Entity
{
    private readonly List<MediaItem> _media = [];

    public int Order { get; private set; }
    public string Narration { get; private set; }
    public Coordinates Location { get; private set; }
    public IReadOnlyList<MediaItem> Media => _media;

    private KeyPoint()
    {
        Narration = null!;
        Location = null!;
    }

    public KeyPoint(int order, string narration, Coordinates location, MediaItem firstMedia) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(narration))
        {
            throw new DomainException("A key point requires a narration.");
        }

        Order = order;
        Narration = narration;
        Location = location;
        _media.Add(firstMedia);
    }
}
