using Application.Common.Caching;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TourBookings.Cache;
using Application.Features.TourBookings.DTOs;
using Application.Features.TourSchedules.Cache;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourBookings.Commands.CreateTourBooking;

public sealed record CreateTourBookingCommand(
    long UserId,
    long TourScheduleId,
    int  AdultsCount,
    int  ChildrenCount,
    int  InfantsCount
) : IRequest<ApiResponse<TourBookingResponse>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class CreateTourBookingCommandValidator : AbstractValidator<CreateTourBookingCommand>
{
    public CreateTourBookingCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.TourScheduleId)
            .GreaterThan(0).WithMessage("TourScheduleId must be a valid ID.");

        RuleFor(x => x.AdultsCount)
            .GreaterThanOrEqualTo(1).WithMessage("At least one adult is required.");

        RuleFor(x => x.ChildrenCount)
            .GreaterThanOrEqualTo(0).WithMessage("Children count cannot be negative.");

        RuleFor(x => x.InfantsCount)
            .GreaterThanOrEqualTo(0).WithMessage("Infants count cannot be negative.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class CreateTourBookingCommandHandler
    : IRequestHandler<CreateTourBookingCommand, ApiResponse<TourBookingResponse>>
{
    private readonly IUnitOfWork   _uow;
    private readonly ICacheService _cache;

    public CreateTourBookingCommandHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow   = uow;
        _cache = cache;
    }

    public async Task<ApiResponse<TourBookingResponse>> Handle(
        CreateTourBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify passenger exists
        var passenger = await _uow.Repository<passenger>()
            .GetByIdAsync(request.UserId, cancellationToken);

        if (passenger is null)
            throw new NotFoundException(nameof(passenger), request.UserId);

        // 2. Load schedule with price tier and tour
        var schedule = await _uow.Repository<tour_schedule>().Query()
            .Include(s => s.price_tier)
            .Include(s => s.tour)
            .FirstOrDefaultAsync(s => s.id == request.TourScheduleId, cancellationToken);

        if (schedule is null)
            throw new NotFoundException(nameof(tour_schedule), request.TourScheduleId);

        if (schedule.is_cancelled)
            throw new ConflictException("This tour schedule has been cancelled and is no longer accepting bookings.");

        if (schedule.tour.is_deleted || schedule.tour.status != TourStatus.Active)
            throw new BadRequestException("This tour is no longer available for booking.");

        // Check for duplicate active bookings for this schedule by this user
        var existingBooking = await _uow.Repository<Domain.Entities.booking>().Query()
            .Include(b => b.tour_booking)
            .Where(b => b.user_id == request.UserId && 
                        b.category == "tour" &&
                        b.tour_booking.tour_schedule_id == request.TourScheduleId &&
                        b.status != BookingStatus.Cancelled.ToString() && 
                        b.IsCancelled != true)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingBooking != null)
            throw new ConflictException("You already have an active booking for this schedule.");

        // 3. Validate schedule is in the future
        if (schedule.start_date <= DateTime.UtcNow)
            throw new BadRequestException("Cannot book a tour schedule that has already started or passed.");

        // 4. Check availability
        var totalGuests = request.AdultsCount + request.ChildrenCount + request.InfantsCount;

        if (schedule.available_slots < totalGuests)
            throw new BadRequestException(
                $"Not enough available slots. Requested: {totalGuests}, Available: {schedule.available_slots}.");

        // 5. Calculate pricing
        var priceTier = schedule.price_tier;
        var subtotal  =
            (request.AdultsCount   * priceTier.adult_price) +
            (request.ChildrenCount * (priceTier.child_price  ?? 0m)) +
            (request.InfantsCount  * (priceTier.infant_price ?? 0m));

        var totalPrice = subtotal; // no coupon discount at this stage

        // 6. Generate booking number
        var bookingNumber = "TOUR-" + Guid.NewGuid().ToString("N")[..8].ToUpper();


        // 7. Create parent booking
        var parentBooking = new booking
        {
            booking_number  = bookingNumber,
            user_id         = request.UserId,
            category        = "tour",
            status          = BookingStatus.Confirmed.ToString(), 
            subtotal        = subtotal,
            total_price     = totalPrice,
            discount_amount = 0m,
            currency        = priceTier.currency,
            payment_status  = "pending",
            created_at      = DateTime.UtcNow
        };

        await _uow.Repository<Domain.Entities.booking>().AddAsync(parentBooking, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // 8. Create tour booking linked to the parent
        var tourBooking = new tour_booking
        {
            booking_id       = parentBooking.id,
            tour_schedule_id = request.TourScheduleId,
            adults_count     = request.AdultsCount,
            children_count   = request.ChildrenCount,
            infants_count    = request.InfantsCount
        };

        await _uow.Repository<tour_booking>().AddAsync(tourBooking, cancellationToken);

        // 9. Decrement available slots
        schedule.available_slots -= totalGuests;
        _uow.Repository<tour_schedule>().Update(schedule);

        await _uow.SaveChangesAsync(cancellationToken);

        // 10. Build response.
        // Manual construction is intentional — we already hold all the loaded entities in memory;
        // re-fetching with includes to use AutoMapper would cost an extra DB round-trip with no benefit.
        var response = new TourBookingResponse
        {
            BookingId         = parentBooking.id,
            BookingNumber     = parentBooking.booking_number,
            Status            = parentBooking.status?.ToString(),
            TourTitle         = schedule.tour.title,
            TourSlug          = schedule.tour.slug,
            TourMainImageUrl  = schedule.tour.main_image_url,
            ScheduleStartDate = schedule.start_date,
            ScheduleEndDate   = schedule.end_date,
            AdultsCount       = request.AdultsCount,
            ChildrenCount     = request.ChildrenCount,
            InfantsCount      = request.InfantsCount,
            PriceTierName     = priceTier.name,
            AdultPrice        = priceTier.adult_price,
            ChildPrice        = priceTier.child_price,
            InfantPrice       = priceTier.infant_price,
            Subtotal          = subtotal,
            TotalPrice        = totalPrice,
            Currency          = priceTier.currency,
            PaymentStatus     = parentBooking.payment_status,
            CreatedAt         = parentBooking.created_at
        };

        // Invalidate booking list for this user and the schedule availability cache.
        await _cache.RemoveByPrefixAsync(TourBookingCacheKeys.UserPrefix(request.UserId), cancellationToken);
        await _cache.RemoveAsync(TourScheduleCacheKeys.ById(request.TourScheduleId), cancellationToken);

        return ApiResponse<TourBookingResponse>.Ok(response, "Tour booking created successfully.");
    }
}
