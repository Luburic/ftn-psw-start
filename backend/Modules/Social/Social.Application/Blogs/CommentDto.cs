namespace Social.Application.Blogs;

public sealed record CommentDto(Guid Id, Guid UserId, string Text, DateTime CreatedAt);
