using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.FlightBookings.DTOs;
using Application.Features.Flights.Caching;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.FlightBookings.Commands.UpdateFlightBooking;

public sealed record UpdateFlightBookingCommand(
    long Id,
    long FlightId,
    long? ReturnFlightId,
    string TripType,
    IReadOnlyList<FlightBookingPassengerRequest> Passengers)
    : IRequest<ApiResponse<FlightBookingResponse>>;

public sealed class UpdateFlightBookingCommandValidator
    : AbstractValidator<UpdateFlightBookingCommand>
{
    public UpdateFlightBookingCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FlightId).GreaterThan(0);

        RuleFor(x => x.TripType)
            .NotEmpty()
            .Must(x => x is "one_way" or "round_trip")
            .WithMessage("TripType must be one_way or round_trip.");

        RuleFor(x => x.ReturnFlightId)
            .NotNull()
            .When(x => x.TripType == "round_trip")
            .WithMessage("ReturnFlightId is required for round_trip.");

        RuleFor(x => x.ReturnFlightId)
            .Null()
            .When(x => x.TripType == "one_way")
            .WithMessage("ReturnFlightId must be omitted for one_way.");

        RuleFor(x => x)
            .Must(x => x.ReturnFlightId != x.FlightId)
            .When(x => x.ReturnFlightId.HasValue)
            .WithMessage("ReturnFlightId must be different from FlightId.");

        RuleFor(x => x.Passengers)
            .NotEmpty()
            .WithMessage("At least one passenger is required.");

        RuleForEach(x => x.Passengers).ChildRules(passenger =>
        {
            passenger.RuleFor(x => x.Title)
                .MaximumLength(5)
                .When(x => x.Title is not null);
            passenger.RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            passenger.RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            passenger.RuleFor(x => x.PassportNumber)
                .MaximumLength(30)
                .When(x => x.PassportNumber is not null);
        });
    }
}

public sealed class UpdateFlightBookingCommandHandler
    : IRequestHandler<UpdateFlightBookingCommand, ApiResponse<FlightBookingResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFlightCacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateFlightBookingCommandHandler> _logger;

    public UpdateFlightBookingCommandHandler(
        IApplicationDbContext context,
        IFlightCacheService cache,
        IMapper mapper,
        ILogger<UpdateFlightBookingCommandHandler> logger)
    {
        _context = context;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the update of a flight booking based on the provided command. It performs various validations, checks for seat availability, and updates the flight booking details accordingly.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ApiResponse<FlightBookingResponse>> Handle(
        UpdateFlightBookingCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating flight booking {BookingId} to FlightId {FlightId} with {PassengerCount} passengers",
            request.Id,
            request.FlightId,
            request.Passengers.Count);

        // Check if the flight booking exists and retrieve it along with related entities
        var flightBooking = await _context.flight_bookings
            .Include(x => x.booking)
            .Include(x => x.flight_booking_passengers)
            .FirstOrDefaultAsync(x => x.id == request.Id, cancellationToken);

        if (flightBooking is null)
        {
            _logger.LogWarning(
                "Flight booking {BookingId} was not found for update",
                request.Id);
            return ApiResponse<FlightBookingResponse>.Fail("Flight booking not found.", 404);
        }

        var oldFlightId = flightBooking.flight_id;
        var oldReturnFlightId = flightBooking.return_flight_id;

        if (!string.Equals(flightBooking.booking.payment_status, "unpaid", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Paid flight booking {BookingId} cannot be updated",
                request.Id);
            return ApiResponse<FlightBookingResponse>.Fail("A paid flight booking cannot be updated.", 409);
        }

        if (flightBooking.booking.IsCancelled == true ||
            string.Equals(flightBooking.booking.status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(flightBooking.booking.status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<FlightBookingResponse>.Fail(
                "A cancelled or completed flight booking cannot be updated.",
                409);
        }

        // Retrieve the relevant flights (outbound and return) based on the flight IDs
        var flightIds = new long?[]
            {
                flightBooking.flight_id,
                flightBooking.return_flight_id,
                request.FlightId,
                request.ReturnFlightId
            }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        // Retrieve the flights from the database and store them in a dictionary for easy access
        var flights = await _context.flights
            .Where(x => flightIds.Contains(x.id))
            .ToDictionaryAsync(x => x.id, cancellationToken);

        // Validate the outbound flight
        if (!flights.TryGetValue(request.FlightId, out var outboundFlight))
            return ApiResponse<FlightBookingResponse>.Fail("Flight not found.", 404);

        // Validate the status of the outbound flight
        if (outboundFlight.status != "scheduled")
            return ApiResponse<FlightBookingResponse>.Fail("Flight is not available for booking.", 409);

        // Validate the return flight if it is provided
        flight? returnFlight = null;
        if (request.ReturnFlightId.HasValue)
        {
            if (!flights.TryGetValue(request.ReturnFlightId.Value, out returnFlight))
                return ApiResponse<FlightBookingResponse>.Fail("Return flight not found.", 404);

            if (returnFlight.status != "scheduled")
                return ApiResponse<FlightBookingResponse>.Fail(
                    "Return flight is not available for booking.",
                    409);
        }

        // Restore the seats for the old flights before checking availability for the new flights 
        var oldPassengerCount = flightBooking.flight_booking_passengers.Count;
        var newPassengerCount = request.Passengers.Count;

        // Restore the seats for the old flights before checking availability for the new flights
        RestoreSeats(flights, flightBooking.flight_id, oldPassengerCount);
        if (flightBooking.return_flight_id.HasValue)
            RestoreSeats(flights, flightBooking.return_flight_id.Value, oldPassengerCount);

        if (outboundFlight.seats_available < newPassengerCount)
            return ApiResponse<FlightBookingResponse>.Fail("Not enough seats available.", 409);

        if (returnFlight is not null && returnFlight.seats_available < newPassengerCount)
            return ApiResponse<FlightBookingResponse>.Fail(
                "Not enough seats available on return flight.",
                409);

        // Update the seats available for the new flights after validation
        outboundFlight.seats_available -= newPassengerCount;
        if (returnFlight is not null)
            returnFlight.seats_available -= newPassengerCount;

        // Calculate the subtotal based on the new passenger count and flight prices
        var subtotal = outboundFlight.base_price * newPassengerCount;
        if (returnFlight is not null)
            subtotal += returnFlight.base_price * newPassengerCount;

        // Update the flight booking details with the new information
        _context.flight_booking_passengers.RemoveRange(
            flightBooking.flight_booking_passengers);

        // Update the flight booking passengers with the new passenger details
        flightBooking.flight_booking_passengers = request.Passengers
            .Select(x => new flight_booking_passenger
            {
                title = x.Title,
                first_name = x.FirstName,
                last_name = x.LastName,
                passport_number = x.PassportNumber
            })
            .ToList();

        // Update the flight booking with the new flight IDs, trip type, subtotal and other relevant details
        flightBooking.flight_id = outboundFlight.id;
        flightBooking.return_flight_id = returnFlight?.id;
        flightBooking.trip_type = request.TripType;
        flightBooking.price = subtotal;

        flightBooking.booking.subtotal = subtotal;
        flightBooking.booking.total_price =
            Math.Max(0, subtotal - flightBooking.booking.discount_amount);
        flightBooking.booking.currency = outboundFlight.currency;
        flightBooking.booking.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<FlightBookingResponse>(flightBooking);

        foreach (var flightId in new long?[]
                 {
                     oldFlightId,
                     oldReturnFlightId,
                     outboundFlight.id,
                     returnFlight?.id
                 }
                 .Where(id => id.HasValue)
                 .Select(id => id!.Value)
                 .Distinct())
        {
            _cache.Remove(FlightCacheKeys.FlightDetails(flightId));
        }

        _cache.RemoveByPrefix(FlightCacheKeys.FlightSearchPrefix);
        _cache.Set(
            FlightCacheKeys.FlightBookingDetails(request.Id),
            response,
            TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "Flight booking {BookingId} updated successfully",
            request.Id);

        return ApiResponse<FlightBookingResponse>.Ok(
            response,
            "Flight booking updated successfully.");
    }

    /// <summary>
    /// Restores the seats for a flight by adding back the specified number of passengers to the available seats.
    /// </summary>
    /// <param name="flights"></param>
    /// <param name="flightId"></param>
    /// <param name="passengerCount"></param>
    private static void RestoreSeats(
        IReadOnlyDictionary<long, flight> flights,
        long flightId,
        int passengerCount)
    {
        if (flights.TryGetValue(flightId, out var flight))
            flight.seats_available += passengerCount;
    }
}
