using Amazon;
using Amazon.S3;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Services;
using Infrastructure.AWSSettings;
using Infrastructure.Caching;
using Infrastructure.Payments;
using Infrastructure.Persistence;
using Infrastructure.RateLimiting;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        // ── Caching (IMemoryCache + ICacheService + CacheSettings validation) ──
        services.AddApplicationCaching(configuration);

        // ── Rate Limiting (fixed-window policies + RateLimiterSettings validation) ──
        services.AddApplicationRateLimiting(configuration);

        services.AddTransient<ICalculateNightPrice, CalculateNightPrice>();
        services.AddTransient<ICheckAvailabilityRoom, CheckAvailabilityRoom>();
        services.AddTransient<ICalculateNumberOfNights, CalculateNumberOfNights>();
        services.AddTransient<IGenerateSlug, GenerateSlug>();
        services.AddTransient(typeof(ICachService<>), typeof(CachService<>));
        services.AddTransient<IBookingService, BookingService>();
        services.AddSingleton<IFlightCacheService, FlightMemoryCacheService>();

        //. AWS registeration 

        services.AddAWSService<IAmazonS3>();

        //. Images Service Registeration's DI 
        services.AddTransient<IAWSImageService, AWSImageService>();
        services.AddTransient<IValidateRequest, ValidateRequest>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IHotelBookingRepository, HotelBookingRepository>();

        services.AddSingleton<ICacheInvalidationService, CacheInvalidationService>();

        services.AddMemoryCache();
        // ── Security & JWT ───────────────────────────────────
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtSettings));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        //---- Stripe Payment Service-------------------------------
        services.Configure<StripeSettings>(
            configuration.GetSection(StripeSettings.SectionName));

        services.AddScoped<IStripeService, StripeService>();

        services.AddScoped<IPaymentGatewayService, StripePaymentGatewayService>();

        return services;
    }
}
