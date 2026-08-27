using Social.Domain.Blogs;

namespace Social.Application.Blogs;

public sealed record BlogDto(
    Guid Id,
    Guid AuthorId,
    string Title,
    string Description,
    DateTime CreatedAt,
    List<string> Images,
    BlogStatus Status,
    List<CommentDto> Comments);
