using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<ApiResponse<AuthResponse>>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly HybridCache _cache;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        HybridCache cache)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _cache = cache;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // NOTE: refreshToken is a random GUID — not suitable as a cache key.
        // We look it up from the DB, rotate it, and then evict the email-keyed profile.

        // 1. Find passenger with matching refresh token
        var user = await _context.passengers
            .Include(p => p.role)
            .FirstOrDefaultAsync(p => p.refreshToken == request.RefreshToken, cancellationToken);

        if (user is null)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid refresh token.", statusCode: 400);
        }

        // 2. Check if token is expired
        if (DateTime.TryParse(user.refreshToken, out var expiry) && expiry < DateTime.UtcNow)
        {
            // Clear expired token details
            user.refreshToken = null;
            user.refresh_token_expiry = default ;
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<AuthResponse>.Fail("Refresh token has expired. Please login again.", statusCode: 400);
        }

        // 3. Generate new JWT token and rotate refresh token
        var newAccessToken = _jwtTokenGenerator.GenerateToken(user);
        var newRefreshToken = Guid.NewGuid().ToString("N");

        user.refreshToken = newRefreshToken;
        user.refresh_token_expiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Evict the cached profile — the passenger record has changed (new refresh token).
        //    The next Login call will re-populate the cache from the DB.
        await _cache.RemoveAsync($"passenger-email:{user.email}", cancellationToken);

        return ApiResponse<AuthResponse>.Ok(
            new AuthResponse(newAccessToken, newRefreshToken, user.email, user.name, user.role?.name ?? "Passenger"),
            "Token refreshed successfully."
        );
    }
}
