using Application.Common.Interfaces;
using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Registers all Infrastructure layer services into the DI container.
/// Called from Program.cs: builder.Services.AddInfrastructure(builder.Configuration);
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddTransient<ICalculateNightPrice, CalculateNightPrice>();
        services.AddTransient<ICheckAvailabilityRoom, CheckAvailabilityRoom>();
        services.AddTransient<ICalculateNumberOfNights, CalculateNumberOfNights>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
