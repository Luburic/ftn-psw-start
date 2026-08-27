using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application.BlogCommenting;

public sealed class BlogCommentingService
{
    private readonly IBlogRepository _blogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BlogCommentingService(IBlogRepository blogRepository, IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddCommentAsync(Guid blogId, Guid userId, CreateCommentDto dto)
    {
        var blog = await GetBlogAsync(blogId);

        blog.AddComment(userId, dto.Text);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateCommentAsync(Guid blogId, Guid commentId, Guid userId, UpdateCommentDto dto)
    {
        var blog = await GetBlogAsync(blogId);

        blog.UpdateComment(commentId, userId, dto.Text);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(Guid blogId, Guid commentId, Guid userId)
    {
        var blog = await GetBlogAsync(blogId);

        blog.DeleteComment(commentId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Blog> GetBlogAsync(Guid blogId)
    {
        var blog = await _blogRepository.GetByIdAsync(blogId);
        if (blog is null)
        {
            throw new NotFoundException($"Blog {blogId} does not exist.");
        }
        return blog;
    }
}
