using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Passengers.DTOs;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.Passengers.Commands.CreatePassenger;

public sealed record CreatePassengerCommand(
    string Name,
    string Email,
    string? Phone,
    int    RoleId
) : IRequest<ApiResponse<PassengerResponse>>;

public sealed class CreatePassengerCommandValidator : AbstractValidator<CreatePassengerCommand>
{
    public CreatePassengerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("hotel_name is required.")
            .MaximumLength(150).WithMessage("hotel_name must not exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("RoleId must be a valid role.");
    }
}

public sealed class CreatePassengerCommandHandler
    : IRequestHandler<CreatePassengerCommand, ApiResponse<PassengerResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreatePassengerCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper  = mapper;
    }

    public async Task<ApiResponse<PassengerResponse>> Handle(
        CreatePassengerCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate email
        var emailExists = _context.passengers
            .Any(p => p.email == request.Email);

        if (emailExists)
            throw new ConflictException($"A passenger with email '{request.Email}' already exists.");

        var passenger = new passenger
        {
            name             = request.Name,
            email            = request.Email,
            phone            = request.Phone,
            role_id          = request.RoleId,
            is_email_verified = false,
            status           = "unverified",
            created_at       = DateTime.UtcNow
        };

        await _context.passengers.AddAsync(passenger, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<PassengerResponse>.Ok(
            _mapper.Map<PassengerResponse>(passenger),
            "Passenger created successfully.");
    }
}
