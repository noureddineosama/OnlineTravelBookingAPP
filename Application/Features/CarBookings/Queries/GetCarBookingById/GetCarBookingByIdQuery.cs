using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.CarBookings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CarBookings.Queries.GetCarBookingById;

public sealed record GetCarBookingByIdQuery(long BookingId)
    : IRequest<ApiResponse<CarBookingResponse>>;

public sealed class GetCarBookingByIdQueryHandler
    : IRequestHandler<GetCarBookingByIdQuery, ApiResponse<CarBookingResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetCarBookingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CarBookingResponse>> Handle(
        GetCarBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var parentBooking = await _context.bookings
            .Where(b => b.id == request.BookingId && b.category == "car")
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
            .FirstOrDefaultAsync(cancellationToken);

        if (parentBooking is null)
            return ApiResponse<CarBookingResponse>.Fail(
                $"Car booking with ID '{request.BookingId}' was not found.", 404);

        var cb = parentBooking.car_booking;
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

        var response = new CarBookingResponse
        {
            BookingId       = parentBooking.id,
            BookingNumber   = parentBooking.booking_number,
            Status          = parentBooking.status,
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
            Subtotal        = parentBooking.subtotal,
            ExtrasTotal     = extras.Sum(e => e.Price),
            TotalPrice      = parentBooking.total_price,
            Currency        = parentBooking.currency,
            PaymentStatus   = parentBooking.payment_status,
            Extras          = extras,
            CreatedAt       = parentBooking.created_at
        };

        return ApiResponse<CarBookingResponse>.Ok(response);
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
