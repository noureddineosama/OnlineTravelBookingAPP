using Application.Common.Interfaces;
using Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(
    string RefreshToken
) : IRequest<ApiResponse<string>>;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly HybridCache _cache;

    public LogoutCommandHandler(IApplicationDbContext context, HybridCache cache)
    {
        _context = context;
        _cache   = cache;
    }

    public async Task<ApiResponse<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.passengers
            .FirstOrDefaultAsync(p => p.refreshToken == request.RefreshToken, cancellationToken);

        if (user is null)
        {
            return ApiResponse<string>.Fail("Invalid refresh token or already logged out.", 400);
        }

        // Invalidate the refresh token
        user.refreshToken = null;
        user.refresh_token_expiry = default;

        await _context.SaveChangesAsync(cancellationToken);

        // Evict the cached auth profile so stale data is not served on next login
        await _cache.RemoveAsync($"passenger-email:{user.email}", cancellationToken);

        return ApiResponse<string>.Ok("Logged out successfully.", "Logout successful.");
    }
}
