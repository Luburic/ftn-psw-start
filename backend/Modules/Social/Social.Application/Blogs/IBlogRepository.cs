using Social.Domain.Blogs;

namespace Social.Application.Blogs;

public interface IBlogRepository
{
    Task<Blog?> GetByIdAsync(Guid id);
    void Add(Blog blog);
}
