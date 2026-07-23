namespace Application.Features.Favorites.Cache;

/// <summary>
/// Central registry of every cache key used by the Favorites feature.
/// Keys are user-scoped to prevent data leaking across different users.
/// </summary>
public static class FavoriteCacheKeys
{
    /// <summary>All favorites pages for a specific user (prefix for multi-page invalidation).</summary>
    public static string UserPrefix(long userId) => $"favorite:user:{userId}";

    /// <summary>A specific paginated page of a user's favorites list.</summary>
    public static string UserList(long userId, string? category, int page, int pageSize)
        => $"favorite:user:{userId}:cat{category}:p{page}:ps{pageSize}";

    /// <summary>Whether a specific item is favorited by this user.</summary>
    public static string Check(long userId, string category, long itemId)
        => $"favorite:check:u{userId}:cat{category}:item{itemId}";
}
