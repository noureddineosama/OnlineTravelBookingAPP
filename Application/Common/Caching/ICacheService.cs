using Microsoft.Extensions.Caching.Memory;

namespace Application.Common.Caching;

/// <summary>
/// Generic in-process memory-cache service.
/// All tour/booking/favorites features depend on this abstraction — never on IMemoryCache directly.
/// Implementation lives in Infrastructure.Caching and is injected via DI.
/// </summary>
public interface ICacheService
{
    /// <summary>Returns a cached value, or <c>default</c> when the key is absent.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Stores <paramref name="value"/> under <paramref name="key"/>.</summary>
    Task SetAsync<T>(string key, T value,
        MemoryCacheEntryOptions? options = null,
        CancellationToken ct = default);

    /// <summary>Removes a single cache entry.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Returns the cached value when present.
    /// On a cache miss executes <paramref name="factory"/>, stores the result, and returns it.
    /// Thread-safe: a concurrent miss will run the factory only once (no stampede).
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        MemoryCacheEntryOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Removes all cache entries whose key starts with <paramref name="prefix"/>.
    /// Use this for coarse-grained invalidation (e.g. remove all pages of a list).
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}
