using FluentValidation;

namespace Application.Common.Settings;

// ── Settings POCOs ────────────────────────────────────────────────────────────

/// <summary>
/// Strongly-typed configuration for all fixed-window rate-limiter policies.
/// Bound from the "RateLimiter" section in appsettings.json.
/// </summary>
public sealed class RateLimiterSettings
{
    public const string SectionName = "RateLimiter";

    public RateLimiterPolicySettings TourRead      { get; init; } = new();
    public RateLimiterPolicySettings TourWrite     { get; init; } = new();
    public RateLimiterPolicySettings TourBooking   { get; init; } = new();
    public RateLimiterPolicySettings FavoritesRead { get; init; } = new();
    public RateLimiterPolicySettings FavoritesWrite{ get; init; } = new();
}

/// <summary>Options for a single fixed-window rate-limit policy.</summary>
public sealed class RateLimiterPolicySettings
{
    /// <summary>Maximum requests allowed in the window. Default: 10.</summary>
    public int PermitLimit   { get; init; } = 10;

    /// <summary>Window duration in seconds. Default: 10.</summary>
    public int WindowSeconds { get; init; } = 10;
}

// ── FluentValidation Validators ───────────────────────────────────────────────

/// <summary>Validates a single rate-limiter policy section.</summary>
public sealed class RateLimiterPolicySettingsValidator
    : AbstractValidator<RateLimiterPolicySettings>
{
    /// <summary>
    /// Parameterless constructor — required so <c>AddValidatorsFromAssembly</c> can
    /// register this type in DI without needing a string argument.
    /// Uses generic error messages.
    /// </summary>
    public RateLimiterPolicySettingsValidator() : this("Policy") { }

    /// <summary>
    /// Named constructor — used by <see cref="RateLimiterSettingsValidator"/> via
    /// <c>new RateLimiterPolicySettingsValidator("RateLimiter:TourRead")</c> to produce
    /// precise, section-qualified error messages at startup.
    /// </summary>
    public RateLimiterPolicySettingsValidator(string policyName)
    {
        RuleFor(x => x.PermitLimit)
            .GreaterThan(0).WithMessage($"{policyName}.PermitLimit must be > 0.")
            .LessThanOrEqualTo(10_000).WithMessage($"{policyName}.PermitLimit must be ≤ 10 000.");

        RuleFor(x => x.WindowSeconds)
            .GreaterThan(0).WithMessage($"{policyName}.WindowSeconds must be > 0.")
            .LessThanOrEqualTo(86_400).WithMessage($"{policyName}.WindowSeconds must be ≤ 86 400 (24 h).");
    }
}

/// <summary>
/// Validates the full <see cref="RateLimiterSettings"/> graph at startup.
/// Discovered automatically by <c>AddValidatorsFromAssembly</c>.
/// </summary>
public sealed class RateLimiterSettingsValidator : AbstractValidator<RateLimiterSettings>
{
    public RateLimiterSettingsValidator()
    {
        RuleFor(x => x.TourRead)
            .SetValidator(new RateLimiterPolicySettingsValidator("RateLimiter:TourRead"));

        RuleFor(x => x.TourWrite)
            .SetValidator(new RateLimiterPolicySettingsValidator("RateLimiter:TourWrite"));

        RuleFor(x => x.TourBooking)
            .SetValidator(new RateLimiterPolicySettingsValidator("RateLimiter:TourBooking"));

        RuleFor(x => x.FavoritesRead)
            .SetValidator(new RateLimiterPolicySettingsValidator("RateLimiter:FavoritesRead"));

        RuleFor(x => x.FavoritesWrite)
            .SetValidator(new RateLimiterPolicySettingsValidator("RateLimiter:FavoritesWrite"));
    }
}
