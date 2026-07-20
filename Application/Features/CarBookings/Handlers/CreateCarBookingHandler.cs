using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Common.Services;
using Application.Features.CarBookings.Commands;
using Application.Features.CarBookings.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CarBookings.Handlers
{
    public sealed class CreateCarBookingHandler 
        : IRequestHandler<CreateCarBookingCommand, GenericResult<CarBookingResponse>>
    {
        private readonly HybridCache cash;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentIUserService _currentIUser;
        private readonly ILogger<CreateCarBookingHandler> _logger;

        public CreateCarBookingHandler(
            HybridCache cash,
            IUnitOfWork unitOfWork,
            ICurrentIUserService currentIUser,
            ILogger<CreateCarBookingHandler> logger)
        {
            this.cash = cash;
            _unitOfWork = unitOfWork;
            _currentIUser = currentIUser;
            _logger = logger;
        }

        public async Task<GenericResult<CarBookingResponse>> Handle(
            CreateCarBookingCommand request, CancellationToken cancellationToken)
        {
            // Verify passenger exists
            var passenger = await _unitOfWork.Repository<passenger>()
                .GetByIdAsync(_currentIUser.UserId, cancellationToken);

            if (passenger is null)
                return await Result.FailureAsync<CarBookingResponse>(
                    $"Passenger with ID '{_currentIUser.UserId}' was not found.");

            // Load car with brand, category, and pricing tiers
            var car = await _unitOfWork.Repository<car>()
                .Query()
                .Include(c => c.brand)
                .Include(c => c.car_category)
                .Include(c => c.car_pricing_tiers)
                .FirstOrDefaultAsync(c => c.id == request.requestDTO.car_id, cancellationToken);

            if (car is null)
                return await Result.FailureAsync<CarBookingResponse>(
                    $"Car with ID '{request.requestDTO.car_id}' was not found.");

            // Validate car status is active
            if (car.status != "active")
                return await Result.FailureAsync<CarBookingResponse>(
                    "This car is not available for booking.");

            // Calculate rental hours
            var rentalHours = (int)(request.requestDTO.dropoff_at - request.requestDTO.pickup_at).TotalHours;
            if (rentalHours <= 0)
                return await Result.FailureAsync<CarBookingResponse>(
                    "Dropoff time must be after pickup time.");

            // Select appropriate pricing tier
            var pricingTier = car.car_pricing_tiers
                .Where(t => t.from_hours <= rentalHours && (t.to_hours == null || rentalHours <= t.to_hours))
                .OrderByDescending(t => t.from_hours)
                .FirstOrDefault();

            if (pricingTier is null)
                return await Result.FailureAsync<CarBookingResponse>(
                    $"No pricing tier found for {rentalHours} hours rental.");

            // Calculate subtotal
            var subtotal = rentalHours * pricingTier.price_per_hour;

            // Calculate extras total
            decimal extrasTotal = 0m;
            var carExtras = new List<car_extra>();
            if (request.requestDTO.extras != null && request.requestDTO.extras.Count > 0)
            {
                var extraIds = request.requestDTO.extras.Select(e => (long)e.extra_id).ToList();
                carExtras = await _unitOfWork.Repository<car_extra>()
                    .Query()
                    .Where(e => extraIds.Contains(e.id))
                    .ToListAsync(cancellationToken);

                foreach (var extraItem in request.requestDTO.extras)
                {
                    var extra = carExtras.FirstOrDefault(e => e.id == extraItem.extra_id);
                    if (extra != null)
                    {
                        var extraCost = extraItem.quantity * extra.price;
                        extrasTotal += extraCost;
                    }
                }
            }

            var totalPrice = subtotal + extrasTotal;

            // Create parent booking
            var parentBooking = new booking
            {
                booking_number = BookingNumber.GeneratBookingNumber(),
                user_id = _currentIUser.UserId,
                category = "car",
                status = Domain.Enums.BookingStatus.Confirmed.ToString(),
                subtotal = subtotal,
                discount_amount = 0m,
                total_price = totalPrice,
                currency = "USD",
                payment_status = "pending",
                created_at = DateTime.UtcNow
            };

            await _unitOfWork.Repository<booking>().AddAsync(parentBooking, cancellationToken);

            // Create car booking
            var carBooking = new car_booking
            {
                booking_id          = parentBooking.id,
                car_id              = request.requestDTO.car_id,
                pickup_location_id  = request.requestDTO.pickup_location_id,
                dropoff_location_id = request.requestDTO.dropoff_location_id,
                pickup_at           = request.requestDTO.pickup_at,
                dropoff_at          = request.requestDTO.dropoff_at,
                driver_name         = request.requestDTO.driver_name
            };

            await _unitOfWork.Repository<car_booking>().AddAsync(carBooking, cancellationToken);

            // Create car booking extras
            if (request.requestDTO.extras != null && request.requestDTO.extras.Count > 0)
            {
                foreach (var extraItem in request.requestDTO.extras)
                {
                    var extra = carExtras.FirstOrDefault(e => e.id == extraItem.extra_id);
                    if (extra != null)
                    {
                        var bookingExtra = new car_booking_extra
                        {
                            car_booking_id = carBooking.id,
                            car_extra_id   = extra.id,
                            quantity       = extraItem.quantity,
                            price          = extraItem.quantity * extra.price
                        };

                        await _unitOfWork.Repository<car_booking_extra>().AddAsync(bookingExtra, cancellationToken);
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            // Load locations for response
            var pickupLocation = await _unitOfWork.Repository<location>()
                .GetByIdAsync(request.requestDTO.pickup_location_id, cancellationToken);
            var dropoffLocation = await _unitOfWork.Repository<location>()
                .GetByIdAsync(request.requestDTO.dropoff_location_id, cancellationToken);

            var responseExtras = new List<CarExtraResponse>();
            if (request.requestDTO.extras != null && request.requestDTO.extras.Count > 0)
            {
                foreach (var extraItem in request.requestDTO.extras)
                {
                    var extra = carExtras.FirstOrDefault(e => e.id == extraItem.extra_id);
                    if (extra != null)
                    {
                        responseExtras.Add(new CarExtraResponse
                        {
                            Name     = extra.name,
                            Quantity = extraItem.quantity,
                            Price    = extraItem.quantity * extra.price
                        });
                    }
                }
            }

            // Build response
            var response = new CarBookingResponse
            {
                BookingId         = parentBooking.id,
                BookingNumber     = parentBooking.booking_number,
                Status            = parentBooking.status?.ToString() ?? string.Empty,
                CarId             = car.id,
                CarModel          = car.model,
                CarYear           = car.year,
                CarBrand          = car.brand?.name ?? string.Empty,
                CarCategory       = car.car_category?.name ?? string.Empty,
                SeatsCount        = car.seats_count,
                Transmission      = car.transmission,
                FuelType          = car.fuel_type,
                PickupLocation    = FormatLocation(pickupLocation),
                DropoffLocation   = FormatLocation(dropoffLocation),
                PickupAt          = request.requestDTO.pickup_at,
                DropoffAt         = request.requestDTO.dropoff_at,
                RentalHours       = rentalHours,
                DriverName        = request.requestDTO.driver_name,
                PricePerDay       = car.price_per_day,
                Subtotal          = subtotal,
                ExtrasTotal       = extrasTotal,
                TotalPrice        = totalPrice,
                Currency          = parentBooking.currency,
                PaymentStatus     = parentBooking.payment_status,
                Extras            = responseExtras,
                CreatedAt         = parentBooking.created_at
            };

            
            await cash.RemoveAsync($"my-car-bookings:{_currentIUser.UserId}", cancellationToken);

            return await Result.SuccessAsync(response, "Car booking created successfully.");
        }

        private static string FormatLocation(location? loc)
        {
            if (loc == null) return string.Empty;
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(loc.city)) parts.Add(loc.city);
            if (!string.IsNullOrEmpty(loc.country)) parts.Add(loc.country);
            return string.Join(", ", parts);
        }
    }
}
