using Social.Application.Blogs;

namespace Social.Application.BlogAuthoring;

public sealed class BlogAuthoringQueries
{
    private readonly IBlogReadRepository _blogReadRepository;

    public BlogAuthoringQueries(IBlogReadRepository blogReadRepository)
    {
        _blogReadRepository = blogReadRepository;
    }

    public Task<List<BlogDto>> GetByAuthorAsync(Guid authorId)
    {
        return _blogReadRepository.GetByAuthorAsync(authorId);
    }
}
