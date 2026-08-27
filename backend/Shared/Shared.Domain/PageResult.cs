namespace Shared.Domain;

public sealed record PageResult<T>(List<T> Items, int TotalCount);
