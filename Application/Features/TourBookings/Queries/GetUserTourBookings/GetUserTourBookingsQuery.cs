using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Features.TourBookings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourBookings.Queries.GetUserTourBookings;

/// <summary>
/// Paginated query to retrieve a user's tour bookings with full details.
/// </summary>
public sealed record GetUserTourBookingsQuery : PagedQuery, IRequest<ApiResponse<PagedResult<TourBookingResponse>>>
{
    public long UserId { get; init; }
    public string? Status { get; init; }
}

public sealed class GetUserTourBookingsQueryHandler
    : IRequestHandler<GetUserTourBookingsQuery, ApiResponse<PagedResult<TourBookingResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserTourBookingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<TourBookingResponse>>> Handle(
        GetUserTourBookingsQuery request, CancellationToken cancellationToken)
    {
        // Build the query: bookings → tour_booking → schedule → tour + price_tier
        var query = _context.bookings
            .Where(b => b.user_id == request.UserId && b.category == "tour")
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.tour)
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.price_tier)
            .AsQueryable();

        // Optional status filter
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(b => b.status == request.Status);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Paginate
        var items = await query
            .OrderByDescending(b => b.created_at)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Project to DTOs
        var dtos = items.Select(b =>
        {
            var tb = b.tour_booking;
            var schedule = tb?.tour_schedule;
            var tour = schedule?.tour;
            var priceTier = schedule?.price_tier;

            return new TourBookingResponse
            {
                BookingId         = b.id,
                BookingNumber     = b.booking_number,
                Status            = b.status,
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
                Subtotal          = b.subtotal,
                TotalPrice        = b.total_price,
                Currency          = b.currency,
                PaymentStatus     = b.payment_status,
                CreatedAt         = b.created_at
            };
        }).ToList().AsReadOnly();

        var pagedResult = new PagedResult<TourBookingResponse>
        {
            Items      = dtos,
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize
        };

        return ApiResponse<PagedResult<TourBookingResponse>>.Ok(pagedResult);
    }
}
