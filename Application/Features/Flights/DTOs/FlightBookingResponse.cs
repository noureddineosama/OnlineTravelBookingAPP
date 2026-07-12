namespace Application.Features.FlightBookings.DTOs;

public sealed class FlightBookingResponse
{
    public long Id { get; init; }
    public long BookingId { get; init; }
    public string BookingNumber { get; init; } = string.Empty;
    public long FlightId { get; init; }
    public long? ReturnFlightId { get; init; }
    public string TripType { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string BookingStatus { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;

    public IReadOnlyList<FlightBookingPassengerResponse> Passengers { get; init; }
        = Array.Empty<FlightBookingPassengerResponse>();
}