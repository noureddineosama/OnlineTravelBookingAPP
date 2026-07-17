using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Features.CarBookings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CarBookings.Queries.GetUserCarBookings;

/// <summary>
/// Paginated query to retrieve a user's car bookings with full details.
/// </summary>
public sealed record GetUserCarBookingsQuery : PagedQuery, IRequest<ApiResponse<PagedResult<CarBookingResponse>>>
{
    public long UserId { get; init; }
    public string? Status { get; init; }
}

public sealed class GetUserCarBookingsQueryHandler
    : IRequestHandler<GetUserCarBookingsQuery, ApiResponse<PagedResult<CarBookingResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserCarBookingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<CarBookingResponse>>> Handle(
        GetUserCarBookingsQuery request, CancellationToken cancellationToken)
    {
        // Build the query: bookings → car_booking → car + locations + extras
        var query = _context.bookings
            .Where(b => b.user_id == request.UserId && b.category == "car")
            .Include(b => b.car_booking)
                .ThenInclude(cb => cb.car)
                    .ThenInclude(c => c.brand)
            .Include(b => b.car_booking)
                .ThenInclude(cb => cb.car)
                    .ThenInclude(c => c.car_category)
            .Include(b => b.car_booking)
                .ThenInclude(cb => cb.pickup_location)
            .Include(b => b.car_booking)
                .ThenInclude(cb => cb.dropoff_location)
            .Include(b => b.car_booking)
                .ThenInclude(cb => cb.car_booking_extras)
                    .ThenInclude(e => e.car_extra)
            .AsQueryable();

        // Optional status filter
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<Domain.Enums.BookingStatus>(request.Status, true, out var statusEnum))
            query = query.Where(b => b.status == statusEnum.ToString());

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
            var cb  = b.car_booking;
            var car = cb?.car;

            var rentalHours = cb is null ? 0
                : (int)(cb.dropoff_at - cb.pickup_at).TotalHours;

            var extras = cb?.car_booking_extras
                .Select(e => new CarExtraResponse
                {
                    Name     = e.car_extra?.name ?? string.Empty,
                    Quantity = e.quantity,
                    Price    = e.price
                }).ToList() ?? new();

            return new CarBookingResponse
            {
                BookingId       = b.id,
                BookingNumber   = b.booking_number,
                Status          = b.status,
                CarId           = car?.id ?? 0,
                CarModel        = car?.model ?? string.Empty,
                CarYear         = car?.year,
                CarBrand        = car?.brand?.name ?? string.Empty,
                CarCategory     = car?.car_category?.name ?? string.Empty,
                SeatsCount      = car?.seats_count ?? 0,
                Transmission    = car?.transmission ?? string.Empty,
                FuelType        = car?.fuel_type ?? string.Empty,
                PickupLocation  = FormatLocation(cb?.pickup_location),
                DropoffLocation = FormatLocation(cb?.dropoff_location),
                PickupAt        = cb?.pickup_at ?? default,
                DropoffAt       = cb?.dropoff_at ?? default,
                RentalHours     = rentalHours,
                DriverName      = cb?.driver_name,
                PricePerDay     = car?.price_per_day ?? 0m,
                Subtotal        = b.subtotal,
                ExtrasTotal     = extras.Sum(e => e.Price),
                TotalPrice      = b.total_price,
                Currency        = b.currency,
                PaymentStatus   = b.payment_status,
                Extras          = extras,
                CreatedAt       = b.created_at
            };
        }).ToList().AsReadOnly();

        var pagedResult = new PagedResult<CarBookingResponse>
        {
            Items      = dtos,
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize
        };

        return ApiResponse<PagedResult<CarBookingResponse>>.Ok(pagedResult);
    }

    private static string FormatLocation(Domain.Entities.location? loc)
    {
        if (loc is null) return string.Empty;
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(loc.city))    parts.Add(loc.city);
        if (!string.IsNullOrEmpty(loc.country)) parts.Add(loc.country);
        return string.Join(", ", parts);
    }
}
