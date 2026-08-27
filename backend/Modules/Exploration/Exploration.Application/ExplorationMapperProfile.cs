using AutoMapper;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application;

public sealed class ExplorationMapperProfile : Profile
{
    public ExplorationMapperProfile()
    {
        CreateMap<Tour, TourDto>();
        CreateMap<TransportTime, TransportTimeDto>();
    }
}
