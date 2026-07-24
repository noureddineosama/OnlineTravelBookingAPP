using Application.Common.RateLimiting;
using Application.Common.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Infrastructure.RateLimiting;

/// <summary>
/// Registers the complete rate-limiting stack as an Infrastructure concern,
/// consistent with how <see cref="CacheRegistration"/> registers caching.
///
/// Responsibilities:
///   1. Bind and validate <see cref="RateLimiterSettings"/> from appsettings.json at startup.
///   2. Register all named fixed-window policies, keyed by caller IP.
///   3. Set the global rejection code to HTTP 429.
///
/// Called from <see cref="DependencyInjection.AddInfrastructure"/> so that
/// Program.cs never needs to reference rate-limiting types directly.
/// </summary>
public static class RateLimitingRegistration
{
    public static IServiceCollection AddApplicationRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── 1. Bind & validate at startup ─────────────────────────────────────
        services.AddOptions<RateLimiterSettings>()
            .Bind(configuration.GetSection(RateLimiterSettings.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<RateLimiterSettings>,
            FluentValidationOptionsAdapter<RateLimiterSettings>>();

        // ── 2. Snapshot settings for policy registration ──────────────────────
        var rl = configuration
            .GetSection(RateLimiterSettings.SectionName)
            .Get<RateLimiterSettings>() ?? new RateLimiterSettings();

        // ── 3. Register all named policies ────────────────────────────────────
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimitingPolicies.TourRead,
                ctx => FixedWindow(Ip(ctx), rl.TourRead));

            options.AddPolicy(RateLimitingPolicies.TourWrite,
                ctx => FixedWindow(Ip(ctx), rl.TourWrite));

            options.AddPolicy(RateLimitingPolicies.TourBooking,
                ctx => FixedWindow(Ip(ctx), rl.TourBooking));

            options.AddPolicy(RateLimitingPolicies.FavoritesRead,
                ctx => FixedWindow(Ip(ctx), rl.FavoritesRead));

            options.AddPolicy(RateLimitingPolicies.FavoritesWrite,
                ctx => FixedWindow(Ip(ctx), rl.FavoritesWrite));

            options.AddPolicy(RateLimitingPolicies.AuthFixedWindow,
                ctx => RateLimitPartition.GetFixedWindowLimiter(Ip(ctx), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit          = 5,
                    Window               = TimeSpan.FromSeconds(10),
                    AutoReplenishment    = true,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit           = 0
                }));

            options.AddPolicy(RateLimitingPolicies.FlightRead,
                ctx => RateLimitPartition.GetFixedWindowLimiter(UserOrIp(ctx), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit          = 60,
                    Window               = TimeSpan.FromMinutes(1),
                    AutoReplenishment    = true,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit           = 0
                }));

            options.AddPolicy(RateLimitingPolicies.FlightWrite,
                ctx => RateLimitPartition.GetFixedWindowLimiter(UserOrIp(ctx), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit          = 10,
                    Window               = TimeSpan.FromMinutes(1),
                    AutoReplenishment    = true,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit           = 0
                }));

            // Fail-fast: never queue — return 429 immediately.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string Ip(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

    private static RateLimitPartition<string> FixedWindow(
        string key, RateLimiterPolicySettings p)
        => RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit          = p.PermitLimit,
            Window               = TimeSpan.FromSeconds(p.WindowSeconds),
            AutoReplenishment    = true,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit           = 0
        });

    private static string UserOrIp(HttpContext ctx)
    {
        var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrWhiteSpace(userId)
            ? $"user:{userId}"
            : $"ip:{Ip(ctx)}";
    }
}
