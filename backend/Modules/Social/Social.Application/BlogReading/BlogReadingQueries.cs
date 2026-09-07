using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application.BlogReading;

public sealed class BlogReadingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;

    public BlogReadingQueries(IBlogReadRepository blogReadRepository)
    {
        _blogReadRepository = blogReadRepository;
    }

    public async Task<BlogDto> GetByIdAsync(Guid id)
    {
        var blog = await _blogReadRepository.GetByIdAsync(id);
        if (blog is null || blog.Status != BlogStatus.Published)
        {
            throw new NotFoundException($"Blog {id} does not exist.");
        }

        return blog;
    }

    public Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _blogReadRepository.GetPublishedAsync(page, pageSize);
    }
}
