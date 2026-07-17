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

        // Validation pipeline — runs validators before every handler (Fluent Validation Registering) 
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        
        // AutoMapper — auto-discovers all Profile implementations
        services.AddAutoMapper(assembly);
        services.AddAutoMapper(typeof(HotelProfile).Assembly);
    

        return services;
    }
}
