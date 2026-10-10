using System.Threading.RateLimiting;
using Application.Common.Interfaces;
using Application.Common.RateLimiting;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OnlineTravelBooking.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that targets the real SQL Server database
/// (same connection string as the main app) so integration tests run against
/// actual persisted data. No in-memory replacement, no seeding.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Disable Sentry in tests
                ["Sentry:Dsn"]                = "",
                // JWT – must match what Infrastructure uses
                ["JwtSettings:SecretKEY"]     = "SuperSecretKeyForOnlineTravelBookingAPIProject2026!!!!",
                ["JwtSettings:ExpiryMinutes"] = "60",
                ["JwtSettings:Issuer"]        = "OnlineTravelBookingAPI",
                ["JwtSettings:Audience"]      = "OnlineTravelBookingAPI",
                // Stripe stub
                ["Stripe:SecretKey"]          = "sk_test_stub",
                ["Stripe:PublishableKey"]     = "pk_test_stub",
                ["Stripe:WebhookSecret"]      = "whsec_stub",
                // AWS stub
                ["AWS:AccessKey"]             = "stub",
                ["AWS:SecretKey"]             = "stub",
                ["AWS:BucketName"]            = "stub",
                ["AWS:Region"]                = "eu-north-1",
                // Caching
                ["Cache:DefaultSlidingExpirationMinutes"]   = "5",
                ["Cache:TourListMinutes"]                   = "3",
                ["Cache:TourDetailMinutes"]                 = "5",
                ["Cache:FavoritesListMinutes"]              = "1",
                ["Cache:FavoritesCheckMinutes"]             = "1",
            });
        });

        builder.ConfigureServices(services =>
        {
            // ── Remove real AWS client so no credential lookup is attempted ────
            services.RemoveAll(typeof(Amazon.S3.IAmazonS3));
            services.AddSingleton<Amazon.S3.IAmazonS3>(
                new Moq.Mock<Amazon.S3.IAmazonS3>().Object);

            // ── Disable rate-limiting in test execution ───────────────────────
            services.RemoveAll<Microsoft.Extensions.Options.IConfigureOptions<RateLimiterOptions>>();
            services.AddRateLimiter(options =>
            {
                options.AddPolicy(RateLimitingPolicies.AuthFixedWindow,
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy(RateLimitingPolicies.TourRead,
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy(RateLimitingPolicies.TourWrite,
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy(RateLimitingPolicies.TourBooking,
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy(RateLimitingPolicies.FavoritesRead,
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy(RateLimitingPolicies.FavoritesWrite,
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy(RateLimitingPolicies.FlightRead,
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy(RateLimitingPolicies.FlightWrite,
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
            });
        });
    }

    /// <summary>
    /// Creates an HttpClient pre-authenticated as the given passenger (looked up by email
    /// from the real database). The passenger must already exist in the DB.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string email)
    {
        var client = CreateClient();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = db.passengers
            .Include(p => p.role)
            .FirstOrDefault(p => p.email == email);

        if (user is null)
            throw new InvalidOperationException(
                $"Cannot create authenticated client: passenger '{email}' not found in the real database. " +
                "Please register the user via the API before running this test.");

        var jwtGen = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var token  = jwtGen.GenerateToken(user);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}
