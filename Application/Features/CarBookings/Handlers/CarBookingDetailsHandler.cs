using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.CarBookings.DTOs;
using Application.Features.CarBookings.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CarBookings.Handlers
{
    public sealed class CarBookingDetailsHandler 
        : IRequestHandler<CarBookingDetailsQuery, GenericResult<CarBookingResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HybridCache _cache;

        public CarBookingDetailsHandler(IUnitOfWork unitOfWork, HybridCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache      = cache;
        }

        public async Task<GenericResult<CarBookingResponse>> Handle(
            CarBookingDetailsQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"car-booking-details:{request.id}";

            var response = await _cache.GetOrCreateAsync(
                cacheKey,
                async ct => await FetchBookingDetails(request.id, ct),
                cancellationToken: cancellationToken
            );

            if (response is null)
                return await Result.FailureAsync<CarBookingResponse>(
                    $"Car booking with ID '{request.id}' was not found.");

            return await Result.SuccessAsync(response, "Car booking details retrieved successfully.");
        }

        private async Task<CarBookingResponse?> FetchBookingDetails(long bookingId, CancellationToken ct)
        {
            var parentBooking = await _unitOfWork.Repository<booking>()
                .Query()
                .Where(b => b.id == bookingId && b.category == "car")
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
                .FirstOrDefaultAsync(ct);

            if (parentBooking is null)
                return null;

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

            return new CarBookingResponse
            {
                BookingId       = parentBooking.id,
                BookingNumber   = parentBooking.booking_number,
                Status          = parentBooking.status?.ToString() ?? string.Empty,
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
        }

        private static string FormatLocation(location? loc)
        {
            if (loc is null) return string.Empty;
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(loc.city))    parts.Add(loc.city);
            if (!string.IsNullOrEmpty(loc.country)) parts.Add(loc.country);
            return string.Join(", ", parts);
        }
    }
}
