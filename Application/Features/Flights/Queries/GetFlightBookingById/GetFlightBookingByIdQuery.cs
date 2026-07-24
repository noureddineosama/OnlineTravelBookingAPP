using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.FlightBookings.DTOs;
using Application.Features.Flights.Caching;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.FlightBookings.Queries.GetFlightBookingById;

public sealed record GetFlightBookingByIdQuery(long Id)
    : IRequest<ApiResponse<FlightBookingResponse>>;

public sealed class GetFlightBookingByIdQueryHandler
    : IRequestHandler<GetFlightBookingByIdQuery, ApiResponse<FlightBookingResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFlightCacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFlightBookingByIdQueryHandler> _logger;

    public GetFlightBookingByIdQueryHandler(
        IApplicationDbContext context,
        IFlightCacheService cache,
        IMapper mapper,
        ILogger<GetFlightBookingByIdQueryHandler> logger)
    {
        _context = context;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<FlightBookingResponse>> Handle(
        GetFlightBookingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = FlightCacheKeys.FlightBookingDetails(request.Id);
        if (_cache.TryGet<FlightBookingResponse>(cacheKey, out var cachedBooking) &&
            cachedBooking is not null)
        {
            _logger.LogDebug(
                "Flight booking cache hit for BookingId {BookingId}",
                request.Id);
            return ApiResponse<FlightBookingResponse>.Ok(cachedBooking);
        }

        _logger.LogDebug(
            "Flight booking cache miss for BookingId {BookingId}",
            request.Id);

        var flightBooking = await _context.flight_bookings
            .Include(x => x.booking)
            .Include(x => x.flight_booking_passengers)
            .FirstOrDefaultAsync(x => x.id == request.Id, cancellationToken);

        if (flightBooking is null)
        {
            _logger.LogWarning(
                "Flight booking {BookingId} was not found",
                request.Id);
            throw new NotFoundException(nameof(flight_booking), request.Id);
        }

        var response = _mapper.Map<FlightBookingResponse>(flightBooking);
        _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

        return ApiResponse<FlightBookingResponse>.Ok(response);
    }
}
