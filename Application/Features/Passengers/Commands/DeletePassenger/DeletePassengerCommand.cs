using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using FluentValidation;
using MediatR;

namespace Application.Features.Passengers.Commands.DeletePassenger;

public sealed record DeletePassengerCommand(long Id) : IRequest<ApiResponse<string>>;

public sealed class DeletePassengerCommandValidator : AbstractValidator<DeletePassengerCommand>
{
    public DeletePassengerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Passenger ID must be greater than 0.");
    }
}

public sealed class DeletePassengerCommandHandler
    : IRequestHandler<DeletePassengerCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;

    public DeletePassengerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> Handle(
        DeletePassengerCommand request, CancellationToken cancellationToken)
    {
        var passenger = await _context.passengers.FindAsync([request.Id], cancellationToken);

        if (passenger is null)
            throw new NotFoundException(nameof(passenger), request.Id);

        _context.passengers.Remove(passenger);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok("Deleted.", "Passenger deleted successfully.");
    }
}

