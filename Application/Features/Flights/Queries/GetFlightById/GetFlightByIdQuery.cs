using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Flights.Caching;
using Application.Features.Flights.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Flights.Queries.GetFlightById;

public sealed record GetFlightByIdQuery(long Id)
    : IRequest<ApiResponse<FlightResponse>>;

public sealed class GetFlightByIdQueryHandler
    : IRequestHandler<GetFlightByIdQuery, ApiResponse<FlightResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFlightCacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFlightByIdQueryHandler> _logger;

    public GetFlightByIdQueryHandler(
        IApplicationDbContext context,
        IFlightCacheService cache,
        IMapper mapper,
        ILogger<GetFlightByIdQueryHandler> logger)
    {
        _context = context;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<FlightResponse>> Handle(
        GetFlightByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = FlightCacheKeys.FlightDetails(request.Id);
        if (_cache.TryGet<FlightResponse>(cacheKey, out var cachedFlight) &&
            cachedFlight is not null)
        {
            _logger.LogDebug("Flight cache hit for FlightId {FlightId}", request.Id);
            return ApiResponse<FlightResponse>.Ok(cachedFlight);
        }

        _logger.LogDebug("Flight cache miss for FlightId {FlightId}", request.Id);

        var flight = await _context.flights
            .FindAsync([request.Id], cancellationToken);

        if (flight is null)
        {
            _logger.LogWarning("Flight {FlightId} was not found", request.Id);
            throw new NotFoundException(nameof(flight), request.Id);
        }

        var response = _mapper.Map<FlightResponse>(flight);
        _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

        return ApiResponse<FlightResponse>.Ok(response);
    }
}
