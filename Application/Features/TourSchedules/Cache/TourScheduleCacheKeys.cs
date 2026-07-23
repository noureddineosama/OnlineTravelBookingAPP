namespace Application.Features.TourSchedules.Cache;

/// <summary>
/// Central registry of every cache key used by the TourSchedules feature.
/// </summary>
public static class TourScheduleCacheKeys
{
    /// <summary>All schedules for a specific tour (prefix for multi-entry invalidation).</summary>
    public static string TourPrefix(long tourId) => $"schedule:tour:{tourId}";

    /// <summary>A single schedule by its ID.</summary>
    public static string ById(long scheduleId) => $"schedule:{scheduleId}";
}
