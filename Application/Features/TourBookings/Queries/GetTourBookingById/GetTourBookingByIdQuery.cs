using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TourBookings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourBookings.Queries.GetTourBookingById;

public sealed record GetTourBookingByIdQuery(long BookingId)
    : IRequest<ApiResponse<TourBookingResponse>>;

public sealed class GetTourBookingByIdQueryHandler
    : IRequestHandler<GetTourBookingByIdQuery, ApiResponse<TourBookingResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetTourBookingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TourBookingResponse>> Handle(
        GetTourBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var parentBooking = await _context.bookings
            .Where(b => b.id == request.BookingId && b.category == "tour")
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.tour)
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.price_tier)
            .FirstOrDefaultAsync(cancellationToken);

        if (parentBooking is null)
            throw new NotFoundException("Tour booking", request.BookingId);

        var tb = parentBooking.tour_booking;
        var schedule = tb?.tour_schedule;
        var tour = schedule?.tour;
        var priceTier = schedule?.price_tier;

        var response = new TourBookingResponse
        {
            BookingId         = parentBooking.id,
            BookingNumber     = parentBooking.booking_number,
            Status            = parentBooking.status,
            TourTitle         = tour?.title ?? string.Empty,
            TourSlug          = tour?.slug ?? string.Empty,
            TourMainImageUrl  = tour?.main_image_url,
            ScheduleStartDate = schedule?.start_date ?? default,
            ScheduleEndDate   = schedule?.end_date,
            AdultsCount       = tb?.adults_count ?? 0,
            ChildrenCount     = tb?.children_count ?? 0,
            InfantsCount      = tb?.infants_count ?? 0,
            PriceTierName     = priceTier?.name ?? string.Empty,
            AdultPrice        = priceTier?.adult_price ?? 0m,
            ChildPrice        = priceTier?.child_price,
            InfantPrice       = priceTier?.infant_price,
            Subtotal          = parentBooking.subtotal,
            TotalPrice        = parentBooking.total_price,
            Currency          = parentBooking.currency,
            PaymentStatus     = parentBooking.payment_status,
            CreatedAt         = parentBooking.created_at
        };

        return ApiResponse<TourBookingResponse>.Ok(response);
    }
}
