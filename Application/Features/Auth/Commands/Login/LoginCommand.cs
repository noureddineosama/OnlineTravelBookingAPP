using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<ApiResponse<AuthResponse>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly HybridCache _cache;

    private static readonly HybridCacheEntryOptions AuthCacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        HybridCache cache)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _cache = cache;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Cache the passenger profile by email to avoid repeated DB round-trips.
        // Only non-secret, auth-needed fields are stored in the cached profile.
        var cacheKey = $"passenger-email:{request.Email}";

        var profile = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var dbUser = await _context.passengers
                    .Include(p => p.role)
                    .FirstOrDefaultAsync(p => p.email == request.Email, ct);

                if (dbUser is null) return null;

                return new CachedPassengerProfile(
                    dbUser.id,
                    dbUser.email,
                    dbUser.name,
                    dbUser.password_hash ?? string.Empty,
                    dbUser.role?.name
                );
            },
            AuthCacheOptions,
            cancellationToken: cancellationToken
        );

        if (profile is null)
            return ApiResponse<AuthResponse>.Fail("Invalid credentials.", 401);

        if (string.IsNullOrEmpty(profile.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Authentication method not supported for this account (no password set).", 400);

        if (!_passwordHasher.VerifyPassword(request.Password, profile.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid credentials.", 401);

        // Build a lightweight passenger object so the token generator can consume it
        var tokenUser = new Domain.Entities.passenger
        {
            id            = profile.Id,
            email         = profile.Email,
            name          = profile.Name,
            password_hash = profile.PasswordHash,
            role          = profile.RoleName is not null
                                ? new Domain.Entities.role { name = profile.RoleName }
                                : null
        };

        var token = _jwtTokenGenerator.GenerateToken(tokenUser);

        // Persist the new refresh token so the client can use it to rotate access tokens.
        var refreshToken = Guid.NewGuid().ToString("N");
        var dbUser = await _context.passengers
            .FirstAsync(p => p.email == request.Email, cancellationToken);
        dbUser.refreshToken          = refreshToken;
        dbUser.refresh_token_expiry  = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        // Evict the now-stale cached profile so the next login re-reads from DB.
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        return ApiResponse<AuthResponse>.Ok(
            new AuthResponse(token, refreshToken, profile.Email, profile.Name, profile.RoleName ?? "Passenger"),
            "Login successful."
        );
    }
}

