using Exploration.Application.Tours;

namespace Exploration.Application.TourAuthoring;

public sealed class TourAuthoringQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public TourAuthoringQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return _tourReadRepository.GetByAuthorAsync(authorId);
    }
}
