using Exploration.Domain.Tours;

namespace Exploration.Application.Tours;

public sealed record TourDto(
    Guid Id,
    Guid AuthorId,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    TourStatus Status,
    DateTime? PublishedAt,
    List<TransportTimeDto> TransportTimes);
