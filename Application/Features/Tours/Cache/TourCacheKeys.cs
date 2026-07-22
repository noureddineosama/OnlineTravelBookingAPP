namespace Application.Features.Tours.Cache;

/// <summary>
/// Central registry of every cache key used by the Tours feature.
/// Using static methods instead of magic strings ensures keys are consistent,
/// refactor-safe, and easy to grep.
/// </summary>
public static class TourCacheKeys
{
    // ── List keys ─────────────────────────────────────────────────────────────

    /// <summary>All pages of the tour list (used for prefix invalidation).</summary>
    public const string ListPrefix = "tour:list";

    /// <summary>A specific paginated tour list page with optional filters.</summary>
    public static string List(int page, int pageSize, string? status, string? difficulty, string? search)
        => $"tour:list:p{page}:ps{pageSize}:s{status}:d{difficulty}:q{search}";

    // ── Detail keys ───────────────────────────────────────────────────────────

    /// <summary>A single tour by its database ID.</summary>
    public static string ById(long id)       => $"tour:{id}";

    /// <summary>A single tour by its URL slug.</summary>
    public static string BySlug(string slug) => $"tour:slug:{slug}";
}
