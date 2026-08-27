using Exploration.Domain.Tours;

namespace Exploration.Application.Tours;

public interface ITourRepository
{
    Task<Tour?> GetByIdAsync(Guid id);
    void Add(Tour tour);
}
