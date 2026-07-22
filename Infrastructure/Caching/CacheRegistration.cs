using Application.Common.Caching;
using Application.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Caching;

/// <summary>
/// Extension method that wires the full caching stack into the DI container.
/// Called from <c>Program.cs</c> via <c>builder.Services.AddApplicationCaching(builder.Configuration)</c>.
/// </summary>
public static class CacheRegistration
{
    public static IServiceCollection AddApplicationCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. IMemoryCache — the .NET in-process store
        services.AddMemoryCache();

        // 2. Bind and validate CacheSettings from appsettings.json
        services.AddOptions<CacheSettings>()
            .Bind(configuration.GetSection(CacheSettings.SectionName))
            .ValidateOnStart();

        // FluentValidation adapter validates the settings at startup
        services.AddSingleton<IValidateOptions<CacheSettings>,
            FluentValidationOptionsAdapter<CacheSettings>>();

        // 3. Register ICacheService — singleton because IMemoryCache is singleton
        //    and our key-tracker dictionary must survive the lifetime of the app.
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}
