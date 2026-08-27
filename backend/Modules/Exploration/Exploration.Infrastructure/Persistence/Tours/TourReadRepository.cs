using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize)
    {
        var published = _dbContext.Tours
            .Where(tour => tour.Status == TourStatus.Published)
            .OrderByDescending(tour => tour.PublishedAt);

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(published)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
    {
        return tours
            .AsNoTracking()
            .Select(tour => new TourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList()));
    }
}
