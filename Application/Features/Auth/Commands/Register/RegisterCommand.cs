using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Name,
    string Email,
    string Password,
    string? Phone = null,
    int RoleId = 1
) : IRequest<ApiResponse<AuthResponse>>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.RoleId)
    .GreaterThanOrEqualTo(0).WithMessage("RoleId must be a valid role.");
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _context.passengers
            .AnyAsync(p => p.email == request.Email, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<AuthResponse>.Fail($"A user with email '{request.Email}' already exists.");
        }

        // Verify role exists
        var roleExists = await _context.roles
            .AnyAsync(r => r.id == request.RoleId, cancellationToken);

        if (!roleExists)
        {
            return ApiResponse<AuthResponse>.Fail($"Role with ID {request.RoleId} does not exist.");
        }

        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        var user = new passenger
        {
            name = request.Name,
            email = request.Email,
            password_hash = hashedPassword,
            phone = request.Phone,
            role_id = request.RoleId,
            is_email_verified = false,
            status = "unverified",
            created_at = DateTime.UtcNow
        };

        await _context.passengers.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch user with role included so that the token generator has the role name
        var savedUser = await _context.passengers
            .Include(p => p.role)
            .FirstAsync(p => p.id == user.id, cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(savedUser);

        // Issue a refresh token immediately so the user is auto-logged-in
        var refreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        savedUser.refreshToken = refreshToken;
        savedUser.refresh_token_expiry = DateTime.UtcNow.AddDays(7).ToString("o");
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResponse>.Ok(
            new AuthResponse(token, refreshToken, savedUser.email, savedUser.name),
            "Registration successful."
        );
    }
}
