using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.passengers
            .Include(p => p.role)
            .FirstOrDefaultAsync(p => p.email == request.Email, cancellationToken);

        if (user is null)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid credentials.", 401);
        }

        if (string.IsNullOrEmpty(user.password_hash))
        {
            return ApiResponse<AuthResponse>.Fail("Authentication method not supported for this account (no password set).", 400);
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.password_hash);

        if (!isPasswordValid)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid credentials.", 401);
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return ApiResponse<AuthResponse>.Ok(
            new AuthResponse(token, user.email, user.name),
            "Login successful."
        );
    }
}
