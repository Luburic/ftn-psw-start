using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TransportTime
{
    public TransportMode Transport { get; private init; }
    public int Minutes { get; private init; }

    private TransportTime()
    {
    }

    public TransportTime(TransportMode transport, int minutes)
    {
        if (minutes <= 0)
        {
            throw new DomainException("A transport time requires a positive number of minutes.");
        }

        Transport = transport;
        Minutes = minutes;
    }
}
