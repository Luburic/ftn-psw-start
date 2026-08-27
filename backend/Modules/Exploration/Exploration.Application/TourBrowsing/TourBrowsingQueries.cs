using Exploration.Application.Tours;
using Shared.Domain;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tourReadRepository.GetPublishedAsync(page, pageSize);
    }
}
