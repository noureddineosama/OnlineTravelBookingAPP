using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.FlightBookings.DTOs;
using Application.Features.Flights.Caching;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.FlightBookings.Commands.DeleteFlightBooking;

public sealed record DeleteFlightBookingCommand(long Id)
    : IRequest<ApiResponse<DeleteFlightBookingResponse>>;

public sealed class DeleteFlightBookingCommandHandler
    : IRequestHandler<DeleteFlightBookingCommand, ApiResponse<DeleteFlightBookingResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFlightCacheService _cache;
    private readonly ILogger<DeleteFlightBookingCommandHandler> _logger;

    public DeleteFlightBookingCommandHandler(
        IApplicationDbContext context,
        IFlightCacheService cache,
        ILogger<DeleteFlightBookingCommandHandler> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ApiResponse<DeleteFlightBookingResponse>> Handle(
        DeleteFlightBookingCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting flight booking {BookingId}",
            request.Id);

        // Check if the flight booking exists and retrieve it along with related entities   
        var flightBooking = await _context.flight_bookings
            .Include(x => x.booking)
            .Include(x => x.flight_booking_passengers)
            .FirstOrDefaultAsync(x => x.id == request.Id, cancellationToken);

        // If the flight booking does not exist, return a 404 Not Found response
        if (flightBooking is null)
        {
            _logger.LogWarning(
                "Flight booking {BookingId} was not found for deletion",
                request.Id);
            return ApiResponse<DeleteFlightBookingResponse>.Fail("Flight booking not found.", 404);
        }

        // Check if the flight booking is paid; if it is, return a 409 Conflict response
        if (!string.Equals(flightBooking.booking.payment_status, "unpaid", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Paid flight booking {BookingId} cannot be deleted",
                request.Id);
            return ApiResponse<DeleteFlightBookingResponse>.Fail(
                "A paid flight booking cannot be deleted.",
                409);
        }

        // Check if there are any payment records associated with the flight booking; if there are, return a 409 Conflict response
        var hasPayments = await _context.payments
            .AnyAsync(x => x.booking_id == flightBooking.booking_id, cancellationToken);

        // If there are payment records, return a 409 Conflict response
        if (hasPayments)
        {
            return ApiResponse<DeleteFlightBookingResponse>.Fail(
                "A flight booking with payment records cannot be deleted.",
                409);
        }

        // Retrieve the outbound and return flights associated with the flight booking
        var outboundFlight = await _context.flights
            .FirstOrDefaultAsync(x => x.id == flightBooking.flight_id, cancellationToken);

        // If the flight booking has a return flight, retrieve it; otherwise, set returnFlight to null
        var returnFlight = flightBooking.return_flight_id.HasValue
            ? await _context.flights.FirstOrDefaultAsync(
                x => x.id == flightBooking.return_flight_id.Value,
                cancellationToken)
            : null;

        // Calculate the number of passengers associated with the flight booking
        var passengerCount = flightBooking.flight_booking_passengers.Count;

        // Update the seats available for the outbound and return flights, if they exist
        if (outboundFlight is not null)
            outboundFlight.seats_available += passengerCount;

        // Update the seats available for the return flight, if it exists
        if (returnFlight is not null)
            returnFlight.seats_available += passengerCount;

        // Remove the flight booking passengers, flight booking, and booking records from the database
        var booking = flightBooking.booking;
        _context.flight_booking_passengers.RemoveRange(
            flightBooking.flight_booking_passengers);
        _context.flight_bookings.Remove(flightBooking);
        _context.bookings.Remove(booking);

        await _context.SaveChangesAsync(cancellationToken);

        _cache.Remove(FlightCacheKeys.FlightBookingDetails(request.Id));
        _cache.Remove(FlightCacheKeys.FlightDetails(flightBooking.flight_id));
        if (flightBooking.return_flight_id.HasValue)
        {
            _cache.Remove(
                FlightCacheKeys.FlightDetails(flightBooking.return_flight_id.Value));
        }
        _cache.RemoveByPrefix(FlightCacheKeys.FlightSearchPrefix);

        _logger.LogInformation(
            "Flight booking {BookingId} deleted successfully and {PassengerCount} seats released",
            request.Id,
            passengerCount);

        return ApiResponse<DeleteFlightBookingResponse>.Ok(
            new DeleteFlightBookingResponse
            {
                Id = request.Id,
                Deleted = true
            },
            "Flight booking deleted successfully.");
    }
}
