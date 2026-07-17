using Application.Common.Interfaces;
using Application.Services;
using Infrastructure.Persistence;
using Application.Common.Settings;
using Infrastructure.Payments;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddTransient<ICalculateNightPrice, CalculateNightPrice>();
        services.AddTransient<ICheckAvailabilityRoom, CheckAvailabilityRoom>();
        services.AddTransient<ICalculateNumberOfNights, CalculateNumberOfNights>();
        services.AddTransient<IGenerateSlug, GenerateSlug>();
        services.AddTransient<IAWSImageService, AWSImageService>();
        services.AddTransient<IValidateRequest, ValidateRequest>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        // ── Security & JWT ───────────────────────────────────
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