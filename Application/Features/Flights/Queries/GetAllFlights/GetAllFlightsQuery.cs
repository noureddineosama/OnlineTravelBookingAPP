using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Features.Flights.Caching;
using Application.Features.Flights.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Flights.Queries.GetAllFlights;

/// <summary>
/// Search flights with optional filters and pagination.
/// </summary>
public sealed record GetAllFlightsQuery : PagedQuery, IRequest<ApiResponse<PagedResult<FlightResponse>>>
{
    public string? OriginAirportCode { get; init; }
    public string? DestinationAirportCode { get; init; }
    public DateTime? DepartureDateUtc { get; init; }
    public string? CabinClass { get; init; }
    public int? PassengersCount { get; init; }
}

public sealed class GetAllFlightsQueryHandler
    : IRequestHandler<GetAllFlightsQuery, ApiResponse<PagedResult<FlightResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFlightCacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllFlightsQueryHandler> _logger;

    public GetAllFlightsQueryHandler(
        IApplicationDbContext context,
        IFlightCacheService cache,
        IMapper mapper,
        ILogger<GetAllFlightsQueryHandler> logger)
    {
        _context = context;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAllFlightsQuery to retrieve flights based on optional filters and pagination.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ApiResponse<PagedResult<FlightResponse>>> Handle(
        GetAllFlightsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = FlightCacheKeys.FlightSearch(
            request.Page,
            request.PageSize,
            request.OriginAirportCode,
            request.DestinationAirportCode,
            request.DepartureDateUtc,
            request.CabinClass,
            request.PassengersCount);

        if (_cache.TryGet<PagedResult<FlightResponse>>(cacheKey, out var cachedResult) &&
            cachedResult is not null)
        {
            _logger.LogDebug(
                "Flight search cache hit for Page {Page}, PageSize {PageSize}",
                request.Page,
                request.PageSize);
            return ApiResponse<PagedResult<FlightResponse>>.Ok(cachedResult);
        }

        _logger.LogDebug(
            "Flight search cache miss for Page {Page}, PageSize {PageSize}",
            request.Page,
            request.PageSize);

        var query = _context.flights.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.OriginAirportCode))
        {
            var origin = request.OriginAirportCode.Trim().ToUpper();
            query = query.Where(f => f.origin_airport_code == origin);
        }

        if (!string.IsNullOrWhiteSpace(request.DestinationAirportCode))
        {
            var destination = request.DestinationAirportCode.Trim().ToUpper();
            query = query.Where(f => f.destination_airport_code == destination);
        }

        if (request.DepartureDateUtc.HasValue)
        {
            var start = request.DepartureDateUtc.Value.Date;
            var end = start.AddDays(1);

            query = query.Where(f =>
                f.departure_at_utc >= start &&
                f.departure_at_utc < end);
        }

        if (!string.IsNullOrWhiteSpace(request.CabinClass))
        {
            var cabinClass = request.CabinClass.Trim().ToLower();
            query = query.Where(f => f.cabin_class == cabinClass);
        }

        if (request.PassengersCount.HasValue)
        {
            query = query.Where(f =>
                f.seats_available >= request.PassengersCount.Value);
        }

        query = query.Where(f => f.status == "scheduled");

        var paged = await query
            .OrderBy(f => f.departure_at_utc)
            .ToPagedResultAsync(request, cancellationToken);

        var response = paged.MapTo(items =>
            (IReadOnlyList<FlightResponse>)_mapper.Map<List<FlightResponse>>(items));

        _cache.Set(cacheKey, response, TimeSpan.FromMinutes(2));

        _logger.LogInformation(
            "Flight search returned {ResultCount} of {TotalCount} flights",
            response.Items.Count,
            response.TotalCount);

        return ApiResponse<PagedResult<FlightResponse>>.Ok(response);
    }
}
