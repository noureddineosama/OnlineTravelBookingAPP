using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Passengers.DTOs;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Application.Features.Passengers.Commands.UpdatePassenger;

public sealed record UpdatePassengerCommand(
    long Id,
    string? Name,
    string? Phone,
    string? Status
) : IRequest<ApiResponse<PassengerResponse>>;

public sealed class UpdatePassengerCommandValidator : AbstractValidator<UpdatePassengerCommand>
{
    public UpdatePassengerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Passenger ID must be greater than 0.");

        RuleFor(x => x.Name)
            .MaximumLength(150).WithMessage("hotel_name must not exceed 150 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Status)
            .Must(s => s is "active" or "inactive" or "banned" or "unverified")
            .WithMessage("Status must be one of: active, inactive, banned, unverified.")
            .When(x => x.Status is not null);
    }
}

public sealed class UpdatePassengerCommandHandler
    : IRequestHandler<UpdatePassengerCommand, ApiResponse<PassengerResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePassengerCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper  = mapper;
    }

    public async Task<ApiResponse<PassengerResponse>> Handle(
        UpdatePassengerCommand request, CancellationToken cancellationToken)
    {
        var passenger = await _context.passengers.FindAsync([request.Id], cancellationToken);

        if (passenger is null)
            throw new NotFoundException(nameof(passenger), request.Id);

        if (request.Name is not null)   passenger.name   = request.Name;
        if (request.Phone is not null)  passenger.phone  = request.Phone;
        if (request.Status is not null) passenger.status = request.Status;
        passenger.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<PassengerResponse>.Ok(
            _mapper.Map<PassengerResponse>(passenger), "Passenger updated successfully.");
    }
}
