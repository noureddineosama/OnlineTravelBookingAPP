using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.FlightBookings.DTOs;
using Application.Features.Flights.Caching;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.FlightBookings.Commands.CreateFlightBooking;

public sealed record CreateFlightBookingCommand(
    long UserId,
    long FlightId,
    long? ReturnFlightId,
    string TripType,
    IReadOnlyList<FlightBookingPassengerRequest> Passengers
) : IRequest<ApiResponse<FlightBookingResponse>>;

public sealed class CreateFlightBookingCommandValidator
    : AbstractValidator<CreateFlightBookingCommand>
{
    public CreateFlightBookingCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId is required.");

        RuleFor(x => x.FlightId)
            .GreaterThan(0)
            .WithMessage("FlightId is required.");

        RuleFor(x => x.TripType)
            .NotEmpty()
            .WithMessage("TripType is required.")
            .Must(x => x is "one_way" or "round_trip")
            .WithMessage("TripType must be one_way or round_trip.");

        RuleFor(x => x.ReturnFlightId)
            .NotNull()
            .When(x => x.TripType == "round_trip")
            .WithMessage("ReturnFlightId is required for round_trip.");

        RuleFor(x => x.Passengers)
            .NotEmpty()
            .WithMessage("At least one passenger is required.");

        RuleForEach(x => x.Passengers).ChildRules(passenger =>
        {
            passenger.RuleFor(x => x.Title)
                .MaximumLength(5)
                .When(x => x.Title is not null);

            passenger.RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            passenger.RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            passenger.RuleFor(x => x.PassportNumber)
                .MaximumLength(30)
                .When(x => x.PassportNumber is not null);
        });
    }
}

public sealed class CreateFlightBookingCommandHandler
    : IRequestHandler<CreateFlightBookingCommand, ApiResponse<FlightBookingResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFlightCacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFlightBookingCommandHandler> _logger;

    public CreateFlightBookingCommandHandler(
        IApplicationDbContext context,
        IFlightCacheService cache,
        IMapper mapper,
        ILogger<CreateFlightBookingCommandHandler> logger)
    {
        _context = context;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// this method handles the creation of a flight booking based on the provided command. It performs various checks to ensure that the passenger exists, the flight is available, and there are enough seats for the requested passengers. If all checks pass, it creates a new booking and flight booking record in the database and returns a successful response with the booking details.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ApiResponse<FlightBookingResponse>> Handle(
        CreateFlightBookingCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating flight booking for UserId {UserId}, FlightId {FlightId}, PassengerCount {PassengerCount}",
            request.UserId,
            request.FlightId,
            request.Passengers.Count);

        // Check if the passenger exists in the database
        var passengerExists = await _context.passengers
            .AnyAsync(p => p.id == request.UserId, cancellationToken);

        // If the passenger does not exist, return a failure response
        if (!passengerExists)
        {
            _logger.LogWarning(
                "Flight booking rejected because UserId {UserId} was not found",
                request.UserId);
            return ApiResponse<FlightBookingResponse>.Fail("Passenger account not found.", 404);
        }

        // Retrieve the flight from the database based on the provided FlightId
        var flight = await _context.flights
            .FirstOrDefaultAsync(f => f.id == request.FlightId, cancellationToken);

        // If the flight does not exist, return a failure response
        if (flight is null)
        {
            _logger.LogWarning(
                "Flight booking rejected because FlightId {FlightId} was not found",
                request.FlightId);
            return ApiResponse<FlightBookingResponse>.Fail("Flight not found.", 404);
        }

        // Check if the flight is scheduled and available for booking
        if (flight.status != "scheduled")
            return ApiResponse<FlightBookingResponse>.Fail("Flight is not available for booking.", 409);

        // Get the number of passengers in the booking request
        var passengerCount = request.Passengers.Count;

        // Check if there are enough seats available on the flight for the requested number of passengers
        if (flight.seats_available < passengerCount)
        {
            _logger.LogWarning(
                "Insufficient seats on FlightId {FlightId}. Requested {RequestedSeats}, Available {AvailableSeats}",
                flight.id,
                passengerCount,
                flight.seats_available);
            return ApiResponse<FlightBookingResponse>.Fail("Not enough seats available.", 409);
        }

        // If the trip type is round trip, retrieve the return flight and perform similar checks
        flight? returnFlight = null;

        // If the trip type is round trip, retrieve the return flight and perform similar checks
        if (request.TripType == "round_trip")
        {
            returnFlight = await _context.flights
                .FirstOrDefaultAsync(
                    f => f.id == request.ReturnFlightId,
                    cancellationToken);

            if (returnFlight is null)
                return ApiResponse<FlightBookingResponse>.Fail("Return flight not found.", 404);

            if (returnFlight.status != "scheduled")
                return ApiResponse<FlightBookingResponse>.Fail("Return flight is not available for booking.", 409);

            if (returnFlight.seats_available < passengerCount)
                return ApiResponse<FlightBookingResponse>.Fail("Not enough seats available on return flight.", 409);
        }

        // Calculate the subtotal for the booking based on the base price of the flight(s) and the number of passengers
        var subtotal = flight.base_price * passengerCount;

        // If there is a return flight, add its base price to the subtotal
        if (returnFlight is not null)
            subtotal += returnFlight.base_price * passengerCount;

        // Create a new booking record with the calculated subtotal and other relevant information
        var booking = new booking
        {
            booking_number = GenerateBookingNumber(),
            user_id = request.UserId,
            category = "flight",
            status = BookingStatus.Pending.ToString(),
            subtotal = subtotal,
            discount_amount = 0,
            total_price = subtotal,
            currency = flight.currency,
            payment_status = "unpaid",
            created_at = DateTime.UtcNow
        };

        // Create a new flight booking record that links the booking to the flight(s) and includes passenger details
        var flightBooking = new flight_booking
        {
            booking = booking,
            flight_id = flight.id,
            return_flight_id = returnFlight?.id,
            trip_type = request.TripType,
            price = subtotal,

            flight_booking_passengers = request.Passengers
                .Select(p => new flight_booking_passenger
                {
                    title = p.Title,
                    first_name = p.FirstName,
                    last_name = p.LastName,
                    passport_number = p.PassportNumber
                })
                .ToList()
        };

        // Deduct the number of booked seats from the available seats on the flight(s)
        flight.seats_available -= passengerCount;

        // If there is a return flight, deduct the number of booked seats from its available seats as well
        if (returnFlight is not null)
            returnFlight.seats_available -= passengerCount;

        // Add the booking and flight booking records to the database context and save the changes
        await _context.bookings.AddAsync(booking, cancellationToken);
        await _context.flight_bookings.AddAsync(flightBooking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _context.flight_bookings
            .Include(x => x.booking)
            .Include(x => x.flight_booking_passengers)
            .FirstAsync(x => x.id == flightBooking.id, cancellationToken);

        var response = _mapper.Map<FlightBookingResponse>(saved);

        _cache.Remove(FlightCacheKeys.FlightDetails(flight.id));
        if (returnFlight is not null)
            _cache.Remove(FlightCacheKeys.FlightDetails(returnFlight.id));
        _cache.RemoveByPrefix(FlightCacheKeys.FlightSearchPrefix);
        _cache.Set(
            FlightCacheKeys.FlightBookingDetails(saved.id),
            response,
            TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "Flight booking {BookingId} created successfully with BookingNumber {BookingNumber}",
            saved.id,
            saved.booking.booking_number);

        return ApiResponse<FlightBookingResponse>.Ok(
            response,
            "Flight booked successfully.");
    }

    private static string GenerateBookingNumber()
        => $"FLT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
}
