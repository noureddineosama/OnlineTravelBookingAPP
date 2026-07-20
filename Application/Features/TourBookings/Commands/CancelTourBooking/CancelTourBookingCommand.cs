using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
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
    private readonly IUnitOfWork _uow;

    public CancelTourBookingCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<string>> Handle(
        CancelTourBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the booking and verify ownership
        var parentBooking = await _uow.Repository<Domain.Entities.booking>()
            .GetByIdAsync(b =>
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
        var tourBooking = await _uow.Repository<tour_booking>().Query()
            .Include(tb => tb.tour_schedule)
            .FirstOrDefaultAsync(tb => tb.booking_id == request.BookingId, cancellationToken);

        if (tourBooking is null)
            throw new NotFoundException("Tour booking details", request.BookingId);

        // 4. Cancel the booking
        parentBooking.status      = BookingStatus.Cancelled.ToString();
        parentBooking.IsCancelled = true;
        parentBooking.updated_at  = DateTime.UtcNow;

        // 5. Restore available slots
        var totalGuests = tourBooking.adults_count + tourBooking.children_count + tourBooking.infants_count;
        tourBooking.tour_schedule.available_slots += totalGuests;
        _uow.Repository<tour_schedule>().Update(tourBooking.tour_schedule);
        _uow.Repository<Domain.Entities.booking>().Update(parentBooking);

        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok("Cancelled.", "Tour booking cancelled successfully. Slots have been restored.");
    }
}
