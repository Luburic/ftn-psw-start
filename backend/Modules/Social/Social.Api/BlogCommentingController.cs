using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.BlogCommenting;

namespace Social.Api;

[ApiController]
[Route("api/social/blogs")]
[Authorize]
public sealed class BlogCommentingController : ControllerBase
{
    private readonly BlogCommentingService _commentingService;

    public BlogCommentingController(BlogCommentingService commentingService)
    {
        _commentingService = commentingService;
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult> AddComment(Guid id, CreateCommentDto dto)
    {
        await _commentingService.AddCommentAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/comments/{commentId:guid}")]
    public async Task<ActionResult> UpdateComment(Guid id, Guid commentId, UpdateCommentDto dto)
    {
        await _commentingService.UpdateCommentAsync(id, commentId, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/comments/{commentId:guid}")]
    public async Task<ActionResult> DeleteComment(Guid id, Guid commentId)
    {
        await _commentingService.DeleteCommentAsync(id, commentId, User.GetUserId());
        return NoContent();
    }
}
