using FluentValidation;

namespace Application.Common.Settings;

// ── Settings POCO ─────────────────────────────────────────────────────────────

/// <summary>
/// Strongly-typed configuration for memory-cache expirations.
/// Bound from the "Cache" section in appsettings.json.
/// </summary>
public sealed class CacheSettings
{
    public const string SectionName = "Cache";

    /// <summary>
    /// Fallback sliding expiration (minutes) used by <c>CachingBehavior</c>
    /// when a query returns <c>null</c> for <c>SlidingExpiration</c>.
    /// Default: 5.
    /// </summary>
    public int DefaultSlidingExpirationMinutes { get; init; } = 5;

    /// <summary>Sliding expiration for the GET /api/tours list. Default: 3 min.</summary>
    public int TourListMinutes { get; init; } = 3;

    /// <summary>Sliding expiration for GET /api/tours/{id}. Default: 5 min.</summary>
    public int TourDetailMinutes { get; init; } = 5;

    /// <summary>Sliding expiration for GET /api/favorites. Default: 1 min.</summary>
    public int FavoritesListMinutes { get; init; } = 1;

    /// <summary>Sliding expiration for GET /api/favorites/check. Default: 1 min.</summary>
    public int FavoritesCheckMinutes { get; init; } = 1;
}

// ── FluentValidation Validator ────────────────────────────────────────────────

/// <summary>
/// Validates the <see cref="CacheSettings"/> block at startup.
/// Discovered automatically by <c>AddValidatorsFromAssembly</c>.
/// </summary>
public sealed class CacheSettingsValidator : AbstractValidator<CacheSettings>
{
    public CacheSettingsValidator()
    {
        RuleFor(x => x.DefaultSlidingExpirationMinutes)
            .GreaterThan(0).WithMessage("Cache.DefaultSlidingExpirationMinutes must be > 0.")
            .LessThanOrEqualTo(1440).WithMessage("Cache.DefaultSlidingExpirationMinutes must be ≤ 1440 (24 h).");

        RuleFor(x => x.TourListMinutes)
            .GreaterThan(0).WithMessage("Cache.TourListMinutes must be > 0.")
            .LessThanOrEqualTo(1440);

        RuleFor(x => x.TourDetailMinutes)
            .GreaterThan(0).WithMessage("Cache.TourDetailMinutes must be > 0.")
            .LessThanOrEqualTo(1440);

        RuleFor(x => x.FavoritesListMinutes)
            .GreaterThan(0).WithMessage("Cache.FavoritesListMinutes must be > 0.")
            .LessThanOrEqualTo(60);

        RuleFor(x => x.FavoritesCheckMinutes)
            .GreaterThan(0).WithMessage("Cache.FavoritesCheckMinutes must be > 0.")
            .LessThanOrEqualTo(60);
    }
}
