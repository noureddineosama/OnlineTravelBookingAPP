using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Services;
using Application.Features.CarBookings.DTOs;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace Application.Features.CarBookings.Commands.CreateCarBooking;

public sealed record CreateCarBookingCommand(
    long UserId,
    long CarId,
    int PickupLocationId,
    int DropoffLocationId,
    DateTime PickupAt,
    DateTime DropoffAt,
    string? DriverName,
    List<ExtraItem> Extras
) : IRequest<ApiResponse<CarBookingResponse>>;

public sealed record ExtraItem(int ExtraId, int Quantity);

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class CreateCarBookingCommandValidator : AbstractValidator<CreateCarBookingCommand>
{
    public CreateCarBookingCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.CarId)
            .GreaterThan(0).WithMessage("CarId must be a valid ID.");

        RuleFor(x => x.PickupLocationId)
            .GreaterThan(0).WithMessage("PickupLocationId must be a valid ID.");

        RuleFor(x => x.DropoffLocationId)
            .GreaterThan(0).WithMessage("DropoffLocationId must be a valid ID.");

        RuleFor(x => x.PickupAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Pickup time must be in the future.");

        RuleFor(x => x.DropoffAt)
            .GreaterThan(x => x.PickupAt).WithMessage("Dropoff time must be after pickup time.");

        RuleForEach(x => x.Extras)
            .SetValidator(new ExtraItemValidator());
    }
}

public sealed class ExtraItemValidator : AbstractValidator<ExtraItem>
{
    public ExtraItemValidator()
    {
        RuleFor(x => x.ExtraId)
            .GreaterThan(0).WithMessage("ExtraId must be a valid ID.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class CreateCarBookingCommandHandler
    : IRequestHandler<CreateCarBookingCommand, ApiResponse<CarBookingResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateCarBookingCommandHandler(IApplicationDbContext context) 
    {
        _context = context;
    }

    public async Task<ApiResponse<CarBookingResponse>> Handle(
        CreateCarBookingCommand request, CancellationToken cancellationToken)
    {
        // Verify passenger exists
        var passenger = await _context.passengers
            .FindAsync([request.UserId], cancellationToken);

        if (passenger is null)
            return ApiResponse<CarBookingResponse>.Fail(
                $"Passenger with ID '{request.UserId}' was not found.");

        // Load car with brand, category, and pricing tiers
        var car = await _context.cars
            .Include(c => c.brand)
            .Include(c => c.car_category)
            .Include(c => c.car_pricing_tiers)
            .FirstOrDefaultAsync(c => c.id == request.CarId, cancellationToken);

        if (car is null)
            return ApiResponse<CarBookingResponse>.Fail(
                $"Car with ID '{request.CarId}' was not found.");

        // Validate car status is active
        if (car.status != "active")
            return ApiResponse<CarBookingResponse>.Fail(
                "This car is not available for booking.");

        //  Calculate rental hours
        var rentalHours = (int)(request.DropoffAt - request.PickupAt).TotalHours;
        if (rentalHours <= 0)
            return ApiResponse<CarBookingResponse>.Fail(
                "Dropoff time must be after pickup time.");

        // Select appropriate pricing tier
        var pricingTier = car.car_pricing_tiers
            .Where(t => t.from_hours <= rentalHours && (t.to_hours == null || rentalHours <= t.to_hours))
            .OrderByDescending(t => t.from_hours)
            .FirstOrDefault();

        if (pricingTier is null)
            return ApiResponse<CarBookingResponse>.Fail(
                $"No pricing tier found for {rentalHours} hours rental.");

        //  Calculate subtotal
        var subtotal = rentalHours * pricingTier.price_per_hour;

        //  Calculate extras total
        decimal extrasTotal = 0m;
        if (request.Extras != null && request.Extras.Count > 0)
        {
            var extraIds = request.Extras.Select(e => e.ExtraId).ToList();
            var carExtras = await _context.car_extras
                .Where(e => extraIds.Contains(e.id))
                .ToListAsync(cancellationToken);

            foreach (var extraItem in request.Extras)
            {
                var extra = carExtras.FirstOrDefault(e => e.id == extraItem.ExtraId);
                if (extra != null)
                {
                    var extraCost = extraItem.Quantity * extra.price;
                    extrasTotal += extraCost;
                }
            }
        }

        var totalPrice = subtotal + extrasTotal;

        // Create parent booking
        var parentBooking = new booking
        {
            booking_number = BookingNumber.GeneratBookingNumber(),
            user_id = request.UserId,
            category = "car",
            status = "confirmed",
            subtotal = subtotal,
            discount_amount = 0m,
            total_price = totalPrice,
            currency = "USD",
            payment_status = "pending",
            created_at = DateTime.UtcNow
        };

        await _context.bookings.AddAsync(parentBooking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 10. Create car booking
        var carBooking = new car_booking
        {
            booking_id          = parentBooking.id,
            car_id              = request.CarId,
            pickup_location_id  = request.PickupLocationId,
            dropoff_location_id = request.DropoffLocationId,
            pickup_at           = request.PickupAt,
            dropoff_at          = request.DropoffAt,
            driver_name         = request.DriverName
        };

        await _context.car_bookings.AddAsync(carBooking, cancellationToken);

        // 11. Create car booking extras
        if (request.Extras != null && request.Extras.Count > 0)
        {
            var extraIds = request.Extras.Select(e => e.ExtraId).ToList();
            var carExtras = await _context.car_extras
                .Where(e => extraIds.Contains(e.id))
                .ToListAsync(cancellationToken);

            foreach (var extraItem in request.Extras)
            {
                var extra = carExtras.FirstOrDefault(e => e.id == extraItem.ExtraId);
                if (extra != null)
                {
                    var bookingExtra = new car_booking_extra
                    {
                        car_booking_id = carBooking.id,
                        car_extra_id   = extra.id,
                        quantity       = extraItem.Quantity,
                        price          = extraItem.Quantity * extra.price
                    };

                    await _context.car_booking_extras.AddAsync(bookingExtra, cancellationToken);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // 12. Load locations for response
        var pickupLocation = await _context.locations
            .FindAsync(request.PickupLocationId, cancellationToken);
        var dropoffLocation = await _context.locations
            .FindAsync(request.DropoffLocationId, cancellationToken);

        // 13. Build response
        var response = new CarBookingResponse
        {
            BookingId         = parentBooking.id,
            BookingNumber     = parentBooking.booking_number,
            Status            = parentBooking.status,
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
            PickupAt          = request.PickupAt,
            DropoffAt         = request.DropoffAt,
            RentalHours       = rentalHours,
            DriverName        = request.DriverName,
            PricePerDay       = car.price_per_day,
            Subtotal          = subtotal,
            ExtrasTotal       = extrasTotal,
            TotalPrice        = totalPrice,
            Currency          = parentBooking.currency,
            PaymentStatus     = parentBooking.payment_status,
            CreatedAt         = parentBooking.created_at
        };

        return ApiResponse<CarBookingResponse>.Ok(
            response, "Car booking created successfully.");
    }

    static string FormatLocation(location? loc)
    {
        if (loc == null) return string.Empty;
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(loc.city)) parts.Add(loc.city);
        if (!string.IsNullOrEmpty(loc.country)) parts.Add(loc.country);
        return string.Join(", ", parts);
    }
}