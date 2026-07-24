namespace Application.Common.RateLimiting;

/// <summary>
/// Centralized registry of every rate-limit policy name used across the application.
///
/// Why this exists:
///   Policy names are strings shared between two places:
///     1. <c>Program.cs</c>  — where policies are registered via <c>AddPolicy(name, …)</c>
///     2. Controllers        — where they are applied via <c>[EnableRateLimiting(name)]</c>
///
///   Without constants a typo in either place silently disables limiting with no compile-time
///   error.  Referencing a <c>const string</c> from a class turns that into a build error.
///
/// Architecture note:
///   This file lives in <b>Application</b> (the innermost stable layer) so both the API host
///   (which registers policies in <c>Program.cs</c>) and the controllers (Presentation layer)
///   can reference it without creating an outward dependency.
/// </summary>
public static class RateLimitingPolicies
{
    // ── Tours ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Public read endpoints: GET /api/tours, GET /api/tours/{id}.
    /// 30 requests / 10 s per IP  (configurable in appsettings.json → RateLimiter:TourRead).
    /// </summary>
    public const string TourRead = "tour-read";

    /// <summary>
    /// Admin write endpoints: POST/PUT/DELETE /api/tours + schedule management.
    /// 10 requests / 10 s per IP  (configurable in appsettings.json → RateLimiter:TourWrite).
    /// </summary>
    public const string TourWrite = "tour-write";

    /// <summary>
    /// Passenger booking actions: POST/PUT/Cancel on /api/tour-bookings.
    /// 5 requests / 10 s per IP   (configurable in appsettings.json → RateLimiter:TourBooking).
    /// </summary>
    public const string TourBooking = "tour-booking";

    // ── Favorites ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Read endpoints: GET /api/favorites, GET /api/favorites/check.
    /// 20 requests / 10 s per IP  (configurable in appsettings.json → RateLimiter:FavoritesRead).
    /// </summary>
    public const string FavoritesRead = "favorites-read";

    /// <summary>
    /// Write endpoints: POST /api/favorites, DELETE /api/favorites.
    /// 10 requests / 10 s per IP  (configurable in appsettings.json → RateLimiter:FavoritesWrite).
    /// </summary>
    public const string FavoritesWrite = "favorites-write";

    /// <summary>
    /// Authentication and general endpoints rate limit (5 req/10s).
    /// </summary>
    public const string AuthFixedWindow = "auth-fixed-window";

    // ── Flights ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Flight read endpoints: GET /api/flights, GET /api/flight-bookings/{id}.
    /// 60 requests / 1 min per user (or IP when unauthenticated).
    /// </summary>
    public const string FlightRead = "flight-read";

    /// <summary>
    /// Flight write endpoints: POST/PUT/DELETE on /api/flight-bookings.
    /// 10 requests / 1 min per user (or IP when unauthenticated).
    /// </summary>
    public const string FlightWrite = "flight-write";
}
