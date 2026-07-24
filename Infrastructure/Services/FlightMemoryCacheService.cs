using System.Collections.Concurrent;
using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

/// <summary>
/// IMemoryCache adapter scoped to flight-related cache entries.
/// Tracks only keys created through this service so prefix invalidation is safe.
/// </summary>
public sealed class FlightMemoryCacheService : IFlightCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public FlightMemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryGet<T>(string key, out T? value)
        => _cache.TryGetValue(key, out value);

    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        options.RegisterPostEvictionCallback(
            static (evictedKey, _, _, state) =>
            {
                if (state is ConcurrentDictionary<string, byte> keys &&
                    evictedKey is string key)
                {
                    keys.TryRemove(key, out _);
                }
            },
            _keys);

        _keys[key] = 0;
        _cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _keys.Keys.Where(
                     key => key.StartsWith(prefix, StringComparison.Ordinal)))
        {
            Remove(key);
        }
    }
}
