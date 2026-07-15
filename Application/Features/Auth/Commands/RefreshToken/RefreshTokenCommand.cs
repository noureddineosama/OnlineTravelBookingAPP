using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Find passenger with matching refresh token
        var user = await _context.passengers
            .Include(p => p.role)
            .FirstOrDefaultAsync(p => p.refresh_token == request.RefreshToken, cancellationToken);

        if (user is null)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid refresh token.");
        }

        // 2. Check if token is expired
        if (user.refresh_token_expiry < DateTime.UtcNow)
        {
            // Clear expired token details
            user.refresh_token = null;
            user.refresh_token_expiry = null;
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<AuthResponse>.Fail("Refresh token has expired. Please login again.");
        }

        // 3. Generate new JWT token and rotate refresh token
        var newAccessToken = _jwtTokenGenerator.GenerateToken(user);
        var newRefreshToken = Guid.NewGuid().ToString("N");

        user.refresh_token = newRefreshToken;
        user.refresh_token_expiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResponse>.Ok(
            new AuthResponse(newAccessToken, newRefreshToken, user.email, user.name),
            "Token refreshed successfully."
        );
    }
}
