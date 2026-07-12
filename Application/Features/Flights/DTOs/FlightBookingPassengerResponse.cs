namespace Application.Features.FlightBookings.DTOs;

public sealed class FlightBookingPassengerResponse
{
    public long Id { get; init; }
    public string? Title { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PassportNumber { get; init; }
}