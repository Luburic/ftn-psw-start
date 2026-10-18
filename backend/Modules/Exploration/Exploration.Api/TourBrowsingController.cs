using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize]
public sealed class TourBrowsingController : ControllerBase
{
    private readonly TourBrowsingQueries _browsingQueries;

    public TourBrowsingController(TourBrowsingQueries browsingQueries)
    {
        _browsingQueries = browsingQueries;
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<TourDto>>> GetPublished(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        return await _browsingQueries.GetPublishedAsync(page, pageSize, search);
    }

    [HttpGet("{id:guid}/estimated-durations")]
    public async Task<ActionResult<List<TransportTimeDto>>> GetEstimatedDurations(Guid id)
    {
        return await _browsingQueries.GetEstimatedDurationsAsync(id);
    }
}
