using Shared.Domain;
using Social.Application.Blogs;

namespace Social.Application.BlogReading;

public sealed class BlogReadingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;

    public BlogReadingQueries(IBlogReadRepository blogReadRepository)
    {
        _blogReadRepository = blogReadRepository;
    }

    public Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _blogReadRepository.GetPublishedAsync(page, pageSize);
    }
}
