using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourBookings.Commands.CancelTourBooking;

public sealed record CancelTourBookingCommand(
    long BookingId,
    long UserId
) : IRequest<ApiResponse<string>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class CancelTourBookingCommandValidator : AbstractValidator<CancelTourBookingCommand>
{
    public CancelTourBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId must be a valid ID.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class CancelTourBookingCommandHandler
    : IRequestHandler<CancelTourBookingCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;

    public CancelTourBookingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> Handle(
        CancelTourBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the booking and verify ownership
        var parentBooking = await _context.bookings
            .FirstOrDefaultAsync(b =>
                b.id       == request.BookingId &&
                b.user_id  == request.UserId    &&
                b.category == "tour",
                cancellationToken);

        if (parentBooking is null)
            throw new NotFoundException("Tour booking", request.BookingId);

        // 2. Check if already cancelled
        if (parentBooking.status == BookingStatus.Cancelled.ToString())
            throw new ConflictException("This booking is already cancelled.");

        // 3. Load the associated tour_booking with schedule
        var tourBooking = await _context.tour_bookings
            .Include(tb => tb.tour_schedule)
            .FirstOrDefaultAsync(tb => tb.booking_id == request.BookingId, cancellationToken);

        if (tourBooking is null)
            throw new NotFoundException("Tour booking details", request.BookingId);

        // 4. Cancel the booking
        parentBooking.status     = BookingStatus.Cancelled.ToString();
        parentBooking.updated_at = DateTime.UtcNow;

        // 5. Restore available slots
        var totalGuests = tourBooking.adults_count + tourBooking.children_count + tourBooking.infants_count;
        tourBooking.tour_schedule.available_slots += totalGuests;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok("Cancelled.", "Tour booking cancelled successfully. Slots have been restored.");
    }
}
