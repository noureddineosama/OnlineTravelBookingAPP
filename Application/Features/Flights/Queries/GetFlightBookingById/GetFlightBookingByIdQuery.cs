using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.FlightBookings.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.FlightBookings.Queries.GetFlightBookingById;

public sealed record GetFlightBookingByIdQuery(long Id)
    : IRequest<ApiResponse<FlightBookingResponse>>;

public sealed class GetFlightBookingByIdQueryHandler
    : IRequestHandler<GetFlightBookingByIdQuery, ApiResponse<FlightBookingResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFlightBookingByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<FlightBookingResponse>> Handle(
        GetFlightBookingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var flightBooking = await _context.flight_bookings
            .Include(x => x.booking)
            .Include(x => x.flight_booking_passengers)
            .FirstOrDefaultAsync(x => x.id == request.Id, cancellationToken);

        if (flightBooking is null)
            throw new NotFoundException(nameof(flight_booking), request.Id);

        return ApiResponse<FlightBookingResponse>.Ok(
            _mapper.Map<FlightBookingResponse>(flightBooking));
    }
}