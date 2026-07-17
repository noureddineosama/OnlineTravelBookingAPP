using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CarBookings.Commands.CancelCarBooking;

public sealed record CancelCarBookingCommand(
    long BookingId,
    long UserId
) : IRequest<ApiResponse<string>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class CancelCarBookingCommandValidator : AbstractValidator<CancelCarBookingCommand>
{
    public CancelCarBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId must be a valid ID.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class CancelCarBookingCommandHandler
    : IRequestHandler<CancelCarBookingCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;

    public CancelCarBookingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> Handle(
        CancelCarBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the booking and verify ownership
        var parentBooking = await _context.bookings
            .FirstOrDefaultAsync(b =>
                b.id == request.BookingId &&
                b.user_id == request.UserId &&
                b.category == "car",
                cancellationToken);

        if (parentBooking is null)
            return ApiResponse<string>.Fail(
                $"Car booking with ID '{request.BookingId}' was not found for this user.", 404);

        // 2. Check if already cancelled
        if (parentBooking.status == Domain.Enums.BookingStatus.Cancelled.ToString())
            return ApiResponse<string>.Fail(
                "This booking is already cancelled.", 400);

        // 3. Cancel the booking
        parentBooking.status     = Domain.Enums.BookingStatus.Cancelled.ToString();
        parentBooking.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok(
            "Cancelled.", "Car booking cancelled successfully.");
    }
}