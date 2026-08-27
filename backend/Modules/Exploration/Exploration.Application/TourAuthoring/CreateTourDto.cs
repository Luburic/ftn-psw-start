using Exploration.Domain.Tours;

namespace Exploration.Application.TourAuthoring;

public sealed record CreateTourDto(string Name, string Description, TourDifficulty Difficulty, List<string> Tags);
