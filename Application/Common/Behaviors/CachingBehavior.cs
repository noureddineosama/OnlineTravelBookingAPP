using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Settings;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that transparently caches the responses of any
/// <see cref="ICacheableQuery"/> request via <see cref="ICacheService"/>.
///
/// Pipeline order: Validation → Caching → Handler.
///
/// On cache hit  : returns the cached response without calling the handler.
/// On cache miss : calls the handler, stores the result, returns it.
/// Commands      : never implement <see cref="ICacheableQuery"/>, so this behavior
///                 is a no-op for all write operations.
///
/// Expiration priority:
///   1. <see cref="ICacheableQuery.SlidingExpiration"/> on the query (query-level override).
///   2. <see cref="CacheSettings.DefaultSlidingExpirationMinutes"/> from appsettings.json.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache;
    private readonly CacheSettings _settings;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cache,
        IOptions<CacheSettings> settings,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache    = cache;
        _settings = settings.Value;
        _logger   = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
            return await next(cancellationToken);

        var expiration = cacheableQuery.SlidingExpiration
                         ?? TimeSpan.FromMinutes(_settings.DefaultSlidingExpirationMinutes);

        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiration)
            .SetPriority(CacheItemPriority.Normal);

        return await _cache.GetOrCreateAsync(
            cacheableQuery.CacheKey,
            () => next(cancellationToken),
            options,
            cancellationToken);
    }
}
