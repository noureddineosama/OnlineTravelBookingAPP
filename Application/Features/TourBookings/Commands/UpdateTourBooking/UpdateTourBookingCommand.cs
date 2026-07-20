using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TourBookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourBookings.Commands.UpdateTourBooking;

public sealed record UpdateTourBookingCommand(
    long BookingId,
    long UserId,
    int  AdultsCount,
    int  ChildrenCount,
    int  InfantsCount
) : IRequest<ApiResponse<TourBookingResponse>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class UpdateTourBookingCommandValidator : AbstractValidator<UpdateTourBookingCommand>
{
    public UpdateTourBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId must be a valid ID.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.AdultsCount)
            .GreaterThanOrEqualTo(1).WithMessage("At least one adult is required.");

        RuleFor(x => x.ChildrenCount)
            .GreaterThanOrEqualTo(0).WithMessage("Children count cannot be negative.");

        RuleFor(x => x.InfantsCount)
            .GreaterThanOrEqualTo(0).WithMessage("Infants count cannot be negative.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class UpdateTourBookingCommandHandler
    : IRequestHandler<UpdateTourBookingCommand, ApiResponse<TourBookingResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateTourBookingCommandHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TourBookingResponse>> Handle(
        UpdateTourBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch booking with required includes
        var booking = await _uow.Repository<Domain.Entities.booking>().Query()
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.price_tier)
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.tour)
            .FirstOrDefaultAsync(b => b.id == request.BookingId && b.category == "tour", cancellationToken);

        if (booking == null || booking.user_id != request.UserId)
            throw new NotFoundException("Tour booking", request.BookingId);

        if (booking.status == BookingStatus.Cancelled.ToString() || booking.IsCancelled == true)
            throw new BadRequestException("Cannot update a cancelled booking.");

        var tourBooking = booking.tour_booking;
        var schedule = tourBooking.tour_schedule;

        // 2. Prevent modification if tour has started
        if (schedule.start_date <= DateTime.UtcNow)
            throw new BadRequestException("This tour has already started.");

        // 3. Delta calculation for passenger capacity
        int oldTotalGuests = tourBooking.adults_count + tourBooking.children_count + tourBooking.infants_count;
        int newTotalGuests = request.AdultsCount + request.ChildrenCount + request.InfantsCount;
        int guestDifference = newTotalGuests - oldTotalGuests;

        if (guestDifference > 0 && schedule.available_slots < guestDifference)
        {
            throw new BadRequestException($"Not enough available slots. Requested additional: {guestDifference}, Available: {schedule.available_slots}.");
        }

        // Apply delta to schedule availability
        schedule.available_slots -= guestDifference;

        // 4. Update passenger counts
        tourBooking.adults_count = request.AdultsCount;
        tourBooking.children_count = request.ChildrenCount;
        tourBooking.infants_count = request.InfantsCount;

        // 5. Recalculate pricing
        var priceTier = schedule.price_tier;
        var subtotal = 
            (request.AdultsCount * priceTier.adult_price) +
            (request.ChildrenCount * (priceTier.child_price ?? 0m)) +
            (request.InfantsCount * (priceTier.infant_price ?? 0m));

        booking.subtotal = subtotal;
        booking.total_price = subtotal; // no coupon logic applied yet

        _uow.Repository<tour_schedule>().Update(schedule);
        _uow.Repository<tour_booking>().Update(tourBooking);
        _uow.Repository<Domain.Entities.booking>().Update(booking);

        // 6. Save changes transactionally
        await _uow.SaveChangesAsync(cancellationToken);

        // 7. Map and return response
        return ApiResponse<TourBookingResponse>.Ok(_mapper.Map<TourBookingResponse>(booking), "Booking updated successfully.");
    }
}
