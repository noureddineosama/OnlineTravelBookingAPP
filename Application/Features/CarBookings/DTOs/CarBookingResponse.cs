namespace Application.Features.CarBookings.DTOs;

/// <summary>
/// Output DTO for a car booking. Includes parent booking info, car details, locations, and pricing.
/// </summary>
public sealed class CarBookingResponse
{
    public long BookingId { get; init; }
    public string BookingNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    // Car details
    public long CarId { get; init; }
    public string CarModel { get; init; } = string.Empty;
    public int? CarYear { get; init; }
    public string CarBrand { get; init; } = string.Empty;
    public string CarCategory { get; init; } = string.Empty;
    public int SeatsCount { get; init; }
    public string Transmission { get; init; } = string.Empty;
    public string FuelType { get; init; } = string.Empty;

    // Location details
    public string PickupLocation { get; init; } = string.Empty;
    public string DropoffLocation { get; init; } = string.Empty;

    // Rental period
    public DateTime PickupAt { get; init; }
    public DateTime DropoffAt { get; init; }
    public int RentalHours { get; init; }

    // Driver
    public string? DriverName { get; init; }

    // Pricing
    public decimal PricePerDay { get; init; }
    public decimal Subtotal { get; init; }
    public decimal ExtrasTotal { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;

    // Extras
    public List<CarExtraResponse> Extras { get; init; } = new();

    public DateTime CreatedAt { get; init; }
}

