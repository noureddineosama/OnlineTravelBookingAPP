namespace Application.Features.TourBookings.Cache;

/// <summary>
/// Central registry of every cache key used by the TourBookings feature.
/// </summary>
public static class TourBookingCacheKeys
{
    /// <summary>All bookings for a specific user (prefix for multi-page invalidation).</summary>
    public static string UserPrefix(long userId) => $"booking:user:{userId}";

    /// <summary>A specific page of a user's booking list.</summary>
    public static string UserList(long userId, int page, int pageSize, string? status)
        => $"booking:user:{userId}:p{page}:ps{pageSize}:s{status}";

    /// <summary>A single booking by ID.</summary>
    public static string ById(long bookingId) => $"booking:{bookingId}";
}
