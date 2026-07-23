namespace Application.Common.Interfaces;

/// <summary>
/// Marker interface for MediatR queries that should be cached in memory.
/// Implement this interface on any <c>IRequest&lt;TResponse&gt;</c> query record
/// to opt-in to the <see cref="Application.Common.Behaviors.CachingBehavior{TRequest,TResponse}"/> pipeline.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Unique cache key for this query instance.
    /// Should incorporate all discriminating query parameters so that
    /// different parameter combinations produce different keys.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// How long the cached response should live in memory.
    /// Return <c>null</c> to use the global default configured in <c>CacheSettings</c>.
    /// </summary>
    TimeSpan? SlidingExpiration { get; }
}
