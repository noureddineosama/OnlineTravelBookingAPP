namespace Application.Common.Interfaces;

/// <summary>
/// Cache abstraction used only by the Flights and FlightBookings features.
/// Infrastructure provides the IMemoryCache implementation.
/// </summary>
public interface IFlightCacheService
{
    bool TryGet<T>(string key, out T? value);
    void Set<T>(string key, T value, TimeSpan expiration);
    void Remove(string key);
    void RemoveByPrefix(string prefix);
}
