namespace Application.Features.FlightBookings.DTOs;

public sealed class DeleteFlightBookingResponse
{
    public long Id { get; init; }
    public bool Deleted { get; init; }
}
