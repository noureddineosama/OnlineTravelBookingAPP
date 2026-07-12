namespace Application.Features.Flights.DTOs;

/// <summary>
/// Output DTO for flight search/results.
/// </summary>
public sealed class FlightResponse
{
    public long Id { get; init; }
    public string FlightNumber { get; init; } = string.Empty;
    public string CarrierName { get; init; } = string.Empty;
    public string OriginAirportCode { get; init; } = string.Empty;
    public string OriginCity { get; init; } = string.Empty;
    public string DestinationAirportCode { get; init; } = string.Empty;
    public string DestinationCity { get; init; } = string.Empty;
    public DateTime DepartureAtUtc { get; init; }
    public DateTime ArrivalAtUtc { get; init; }
    public int? DurationMinutes { get; init; }
    public string CabinClass { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int SeatsAvailable { get; init; }
    public string Status { get; init; } = string.Empty;
}