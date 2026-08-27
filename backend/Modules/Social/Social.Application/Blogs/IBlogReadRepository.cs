using Shared.Domain;

namespace Social.Application.Blogs;

public interface IBlogReadRepository
{
    Task<List<BlogDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize);
}
