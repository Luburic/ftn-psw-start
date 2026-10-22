using Exploration.Domain.Tours;

namespace Exploration.Application.Tours;

public interface ITourRepository
{
    Task<Tour?> GetByIdAsync(Guid id);
    Task<List<Tour>> GetByAuthorAsync(Guid authorId);
    void Add(Tour tour);
}
