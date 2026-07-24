using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Application.Common.Settings;

/// <summary>
/// Generic bridge between <see cref="IValidator{T}"/> (FluentValidation) and
/// <see cref="IValidateOptions{T}"/> (ASP.NET Core Options validation pipeline).
///
/// Registered as <b>Singleton</b> to satisfy <see cref="IValidateOptions{T}"/>.
/// To avoid consuming a Scoped <see cref="IValidator{T}"/> from a Singleton,
/// this class accepts <see cref="IServiceProvider"/> and creates a short-lived
/// scope each time validation is invoked (which only happens at startup via
/// <c>ValidateOnStart()</c> — effectively once per app lifetime).
///
/// Register one instance per settings class in your DI module, then call
/// <c>services.AddOptions&lt;T&gt;().BindConfiguration(...).ValidateOnStart()</c>
/// to have the app fail fast on misconfiguration.
/// </summary>
public sealed class FluentValidationOptionsAdapter<T> : IValidateOptions<T>
    where T : class
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationOptionsAdapter(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, T options)
    {
        // Create a scope so we can safely resolve a Scoped IValidator<T>
        // from this Singleton adapter. The scope is disposed immediately after validation.
        using var scope     = _serviceProvider.CreateScope();
        var       validator = scope.ServiceProvider.GetRequiredService<IValidator<T>>();

        var result = validator.Validate(options);

        if (result.IsValid)
            return ValidateOptionsResult.Success;

        var errors = result.Errors
            .Select(e => e.ErrorMessage)
            .ToArray();

        return ValidateOptionsResult.Fail(errors);
    }
}
