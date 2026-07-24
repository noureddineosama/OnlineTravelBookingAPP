using Application.Common.Caching;
using Application.Common.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Infrastructure.Caching;

/// <summary>
/// <see cref="IMemoryCache"/>-backed implementation of <see cref="ICacheService"/>.
///
/// Key design decisions:
/// • A <see cref="ConcurrentDictionary{TKey,TValue}"/> of all tracked keys enables
///   prefix-based bulk invalidation (<see cref="RemoveByPrefixAsync"/>) without
///   exposing IMemoryCache's internal compaction APIs.
/// • <see cref="GetOrCreateAsync{T}"/> uses a <see cref="SemaphoreSlim"/> per key
///   to prevent cache stampedes under concurrent traffic.
/// • All public methods are Task-based so the interface is ready for IDistributedCache
///   migration without consumer code changes.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _settings;
    private readonly ILogger<MemoryCacheService> _logger;

    // Tracks every key this service has written; required for prefix invalidation.
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    // Per-key semaphores prevent multiple concurrent cache misses from all hitting the DB.
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public MemoryCacheService(
        IMemoryCache cache,
        IOptions<CacheSettings> settings,
        ILogger<MemoryCacheService> logger)
    {
        _cache    = cache;
        _settings = settings.Value;
        _logger   = logger;
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value,
        MemoryCacheEntryOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= DefaultOptions();
        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
        _logger.LogDebug("[Cache SET] key={Key}", key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogDebug("[Cache REMOVE] key={Key}", key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        MemoryCacheEntryOptions? options = null,
        CancellationToken ct = default)
    {
        // Fast path — value already cached
        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
        {
            _logger.LogDebug("[Cache HIT] key={Key}", key);
            return cached;
        }

        // Slow path — acquire per-key lock to prevent stampede
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if (_cache.TryGetValue(key, out cached) && cached is not null)
            {
                _logger.LogDebug("[Cache HIT after lock] key={Key}", key);
                return cached;
            }

            _logger.LogDebug("[Cache MISS] key={Key} — invoking factory", key);
            var value = await factory();

            options ??= DefaultOptions();
            _cache.Set(key, value, options);
            _keys.TryAdd(key, 0);

            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var matched = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in matched)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        _logger.LogDebug("[Cache REMOVE PREFIX] prefix={Prefix} — {Count} entries removed",
            prefix, matched.Count);

        return Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private MemoryCacheEntryOptions DefaultOptions()
        => new MemoryCacheEntryOptions()
            .SetSlidingExpiration(
                TimeSpan.FromMinutes(_settings.DefaultSlidingExpirationMinutes))
            .SetPriority(CacheItemPriority.Normal)
            .RegisterPostEvictionCallback((key, _, _, _) =>
                _keys.TryRemove(key.ToString()!, out _));
}
