using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class TourAuthoringController : ControllerBase
{
    private readonly TourAuthoringService _authoringService;
    private readonly TourAuthoringQueries _authoringQueries;

    public TourAuthoringController(
        TourAuthoringService authoringService,
        TourAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpPost]
    public async Task<ActionResult<TourDto>> Create(CreateTourDto dto)
    {
        return await _authoringService.CreateAsync(User.GetUserId(), dto);
    }

    [HttpPost("{id:guid}/transport-times")]
    public async Task<ActionResult> AddTransportTime(Guid id, TransportTimeDto dto)
    {
        await _authoringService.AddTransportTimeAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _authoringService.PublishAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<TourDto>>> GetMine()
    {
        return await _authoringQueries.GetByAuthorAsync(User.GetUserId());
    }
}
