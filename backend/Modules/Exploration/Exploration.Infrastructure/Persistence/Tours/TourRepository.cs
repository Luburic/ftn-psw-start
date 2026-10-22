using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourRepository : ITourRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Tour?> GetByIdAsync(Guid id)
    {
        return _dbContext.Tours.FirstOrDefaultAsync(tour => tour.Id == id);
    }

    public void Add(Tour tour)
    {
        _dbContext.Tours.Add(tour);
    }
    
    public Task<List<Tour>> GetByAuthorAsync(Guid authorId)
    {
        return _dbContext.Tours.Where(tour => tour.AuthorId == authorId).ToListAsync();
    }
}
