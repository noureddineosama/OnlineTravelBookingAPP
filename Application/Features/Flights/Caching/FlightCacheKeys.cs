namespace Application.Features.Flights.Caching;

public static class FlightCacheKeys
{
    public const string FlightSearchPrefix = "flight:search:";

    public static string FlightDetails(long flightId)
        => $"flight:details:{flightId}";

    public static string FlightBookingDetails(long bookingId)
        => $"flight-booking:details:{bookingId}";

    public static string FlightSearch(
        int page,
        int pageSize,
        string? origin,
        string? destination,
        DateTime? departureDateUtc,
        string? cabinClass,
        int? passengersCount)
    {
        var normalizedOrigin = Normalize(origin, upperCase: true);
        var normalizedDestination = Normalize(destination, upperCase: true);
        var normalizedCabin = Normalize(cabinClass, upperCase: false);
        var departureDate = departureDateUtc?.Date.ToString("yyyy-MM-dd") ?? "any";
        var passengers = passengersCount?.ToString() ?? "any";

        return $"{FlightSearchPrefix}{page}:{pageSize}:{normalizedOrigin}:" +
               $"{normalizedDestination}:{departureDate}:{normalizedCabin}:{passengers}";
    }

    private static string Normalize(string? value, bool upperCase)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "any";

        var normalized = value.Trim();
        return upperCase
            ? normalized.ToUpperInvariant()
            : normalized.ToLowerInvariant();
    }
}
