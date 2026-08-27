using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;
using Social.Application.BlogReading;
using Social.Application.Blogs;

namespace Social.Api;

[ApiController]
[Route("api/social/blogs")]
[Authorize]
public sealed class BlogReadingController : ControllerBase
{
    private readonly BlogReadingQueries _readingQueries;

    public BlogReadingController(BlogReadingQueries readingQueries)
    {
        _readingQueries = readingQueries;
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<BlogDto>>> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _readingQueries.GetPublishedAsync(page, pageSize);
    }
}
