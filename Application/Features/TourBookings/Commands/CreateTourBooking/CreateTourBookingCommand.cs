using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TourBookings.DTOs;
using Domain.Entities;
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
    private readonly IApplicationDbContext _context;

    public CreateTourBookingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TourBookingResponse>> Handle(
        CreateTourBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify passenger exists
        var passenger = await _context.passengers
            .FindAsync([request.UserId], cancellationToken);

        if (passenger is null)
            return ApiResponse<TourBookingResponse>.Fail(
                $"Passenger with ID '{request.UserId}' was not found.");

        // 2. Load schedule with price tier and tour
        var schedule = await _context.tour_schedules
            .Include(s => s.price_tier)
            .Include(s => s.tour)
            .FirstOrDefaultAsync(s => s.id == request.TourScheduleId, cancellationToken);

        if (schedule is null)
            return ApiResponse<TourBookingResponse>.Fail(
                $"Tour schedule with ID '{request.TourScheduleId}' was not found.");

        // 3. Validate schedule is in the future
        if (schedule.start_date <= DateTime.UtcNow)
            return ApiResponse<TourBookingResponse>.Fail(
                "Cannot book a tour schedule that has already started or passed.");

        // 4. Check availability
        var totalGuests = request.AdultsCount + request.ChildrenCount + request.InfantsCount;

        if (schedule.available_slots < totalGuests)
            return ApiResponse<TourBookingResponse>.Fail(
                $"Not enough available slots. Requested: {totalGuests}, Available: {schedule.available_slots}.");

        // 5. Calculate pricing
        var priceTier = schedule.price_tier;
        var subtotal =
            (request.AdultsCount * priceTier.adult_price) +
            (request.ChildrenCount * (priceTier.child_price ?? 0m)) +
            (request.InfantsCount * (priceTier.infant_price ?? 0m));

        var totalPrice = subtotal; // No coupon discount initially

        // 6. Generate booking number
        var bookingNumber = "TOUR-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

        // 7. Create parent booking
        var parentBooking = new booking
        {
            booking_number  = bookingNumber,
            user_id         = request.UserId,
            category        = "tour",
            status          = "confirmed",
            subtotal        = subtotal,
            discount_amount = 0m,
            total_price     = totalPrice,
            currency        = priceTier.currency,
            payment_status  = "pending",
            created_at      = DateTime.UtcNow
        };

        await _context.bookings.AddAsync(parentBooking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 8. Create tour booking linked to the parent
        var tourBooking = new tour_booking
        {
            booking_id       = parentBooking.id,
            tour_schedule_id = request.TourScheduleId,
            adults_count     = request.AdultsCount,
            children_count   = request.ChildrenCount,
            infants_count    = request.InfantsCount
        };

        await _context.tour_bookings.AddAsync(tourBooking, cancellationToken);

        // 9. Decrement available slots
        schedule.available_slots -= totalGuests;

        await _context.SaveChangesAsync(cancellationToken);

        // 10. Build response
        var response = new TourBookingResponse
        {
            BookingId         = parentBooking.id,
            BookingNumber     = parentBooking.booking_number,
            Status            = parentBooking.status,
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

        return ApiResponse<TourBookingResponse>.Ok(
            response, "Tour booking created successfully.");
    }
}
