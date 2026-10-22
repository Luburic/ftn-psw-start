using Shared.Domain.Exceptions;
 
namespace Exploration.Domain.Tours;
 
public sealed class TourNameUniquenessChecker
{
    public void EnsureUnique(Tour newTour, List<Tour> existingTours)
    {
        var newName = Normalize(newTour.Name);
        var isTaken = existingTours.Any(tour =>
            tour.AuthorId == newTour.AuthorId && Normalize(tour.Name) == newName);
 
        if (isTaken)
        {
            throw new DomainException("The author already has a tour with this name.");
        }
    }
 
    private static string Normalize(string name)
    {
        return name.Trim().ToLowerInvariant();
    }
}