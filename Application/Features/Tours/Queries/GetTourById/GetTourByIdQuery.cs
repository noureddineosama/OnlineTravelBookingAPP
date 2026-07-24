using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Tours.Cache;
using Application.Features.Tours.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tours.Queries.GetTourById;

/// <summary>Get a single active tour by its ID. Result is cached (see Cache:TourDetailMinutes in appsettings).</summary>
public sealed record GetTourByIdQuery(long Id) : IRequest<ApiResponse<TourDto>>, ICacheableQuery
{
    /// <inheritdoc />
    public string    CacheKey          => TourCacheKeys.ById(Id);

    /// <inheritdoc />
    /// Returns <c>null</c> — expiration is driven by <c>CacheSettings.TourDetailMinutes</c> in appsettings.json.
    public TimeSpan? SlidingExpiration => null;
}

internal sealed class GetTourByIdQueryHandler : IRequestHandler<GetTourByIdQuery, ApiResponse<TourDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetTourByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TourDto>> Handle(GetTourByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _uow.Repository<tour>().Query()
            .AsNoTracking()
            .Include(t => t.tour_price_tiers)
            .Include(t => t.tour_schedules)
            .FirstOrDefaultAsync(t => t.id == request.Id && !t.is_deleted && t.status == Domain.Enums.TourStatus.Active, cancellationToken);

        if (entity == null)
            return ApiResponse<TourDto>.Fail("Tour not found.", 404);

        return ApiResponse<TourDto>.Ok(_mapper.Map<TourDto>(entity));
    }
}
