using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record Coordinates
{
    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Coordinates() { }

    public Coordinates(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("Latitude must be between -90 and 90.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("Longitude must be between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}
