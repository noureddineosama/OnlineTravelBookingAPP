using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Application.Profiles;

namespace Application;

/// <summary>
/// Registers all Application layer services into the DI container.
/// Called from Program.cs: builder.Services.AddApplication();
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR — auto-discovers all IRequestHandler implementations
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation — auto-discovers all AbstractValidator<T> implementations
        services.AddValidatorsFromAssembly(assembly);

        // ── MediatR Pipeline Behaviors (order matters: Validation → Caching → Handler) ──
        // 1. Validation: rejects invalid requests before they reach the cache or handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 2. Caching: serves responses from ICacheService for ICacheableQuery requests
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        // Note: IMemoryCache + ICacheService are registered by Infrastructure via
        // builder.Services.AddApplicationCaching(builder.Configuration) in Program.cs.

        // AutoMapper — auto-discovers all Profile implementations
        services.AddAutoMapper(assembly);

        return services;
    }
}
