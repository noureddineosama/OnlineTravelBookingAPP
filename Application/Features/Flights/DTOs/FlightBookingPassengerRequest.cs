namespace Application.Features.FlightBookings.DTOs;

/// <summary>
/// Passenger data required when creating a flight booking.
/// </summary>
public sealed record FlightBookingPassengerRequest(
    string? Title,
    string FirstName,
    string LastName,
    string? PassportNumber);