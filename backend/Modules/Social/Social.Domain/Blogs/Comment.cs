using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Blogs;

public sealed class Comment : Entity
{
    public Guid UserId { get; private set; }
    public string Text { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Comment()
    {
        Text = null!;
    }

    public Comment(Guid userId, string text) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A comment requires text.");
        }

        UserId = userId;
        Text = text;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A comment requires text.");
        }

        Text = text;
    }
}
