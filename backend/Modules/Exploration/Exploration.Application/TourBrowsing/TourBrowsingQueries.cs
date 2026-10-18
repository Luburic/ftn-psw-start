using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourReadRepository _tourReadRepository;
    private readonly ITourRepository _tourRepository;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository, ITourRepository tourRepository)
    {
        _tourReadRepository = tourReadRepository;
        _tourRepository = tourRepository;
    }

    public Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize, string? search)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tourReadRepository.GetPublishedAsync(page, pageSize, search);
    }

    public async Task<List<TransportTimeDto>> GetEstimatedDurationsAsync(Guid tourId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.Status != TourStatus.Published)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        return tour.GetEstimatedDurations()
            .Select(estimate => new TransportTimeDto(estimate.Transport, estimate.Minutes))
            .ToList();
    }
}
