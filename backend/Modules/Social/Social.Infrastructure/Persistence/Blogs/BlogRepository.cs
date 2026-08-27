using Microsoft.EntityFrameworkCore;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence.Blogs;

internal sealed class BlogRepository : IBlogRepository
{
    private readonly SocialDbContext _dbContext;

    public BlogRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Blog?> GetByIdAsync(Guid id)
    {
        return _dbContext.Blogs.FirstOrDefaultAsync(blog => blog.Id == id);
    }

    public void Add(Blog blog)
    {
        _dbContext.Blogs.Add(blog);
    }
}
