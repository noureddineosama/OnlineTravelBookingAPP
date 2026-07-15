using Application.Common.Interfaces;
using Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public LogoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.passengers
            .FirstOrDefaultAsync(p => p.refresh_token == request.RefreshToken, cancellationToken);

        if (user is null)
        {
            return ApiResponse<string>.Fail("Invalid refresh token or already logged out.");
        }

        // Invalidate the refresh token
        user.refresh_token = null;
        user.refresh_token_expiry = null;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok("Logged out successfully.", "Logout successful.");
    }
}
