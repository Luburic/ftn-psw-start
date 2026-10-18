using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class MediaItem : Entity
{
    public string Url { get; private set; }
    public string Description { get; private set; }

    private MediaItem()
    {
        Url = null!;
        Description = null!;
    }

    public MediaItem(string url, string description) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new DomainException("A media item requires a URL.");
        }

        Url = url;
        Description = description;
    }
}
