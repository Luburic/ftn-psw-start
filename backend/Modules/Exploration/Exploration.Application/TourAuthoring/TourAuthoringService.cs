using AutoMapper;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourAuthoring;

public sealed class TourAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TourAuthoringService(ITourRepository tourRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TourDto> CreateAsync(Guid authorId, CreateTourDto dto)
    {
        var tour = new Tour(authorId, dto.Name, dto.Description, dto.Difficulty, dto.Tags);

        _tourRepository.Add(tour);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TourDto>(tour);
    }

    public async Task AddTransportTimeAsync(Guid tourId, Guid authorId, TransportTimeDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.AddTransportTime(dto.Transport, dto.Minutes);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PublishAsync(Guid tourId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }
}
