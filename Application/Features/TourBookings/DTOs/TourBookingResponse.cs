namespace Application.Features.TourBookings.DTOs;

/// <summary>
/// Output DTO for a tour booking. Includes parent booking info, tour details, and pricing.
/// </summary>
public sealed class TourBookingResponse
{
    public long BookingId { get; init; }
    public string BookingNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    // Tour details
    public string TourTitle { get; init; } = string.Empty;
    public string TourSlug { get; init; } = string.Empty;
    public string? TourMainImageUrl { get; init; }

    // Schedule details
    public DateTime ScheduleStartDate { get; init; }
    public DateTime? ScheduleEndDate { get; init; }

    // Guest counts
    public int AdultsCount { get; init; }
    public int ChildrenCount { get; init; }
    public int InfantsCount { get; init; }

    // Pricing
    public string PriceTierName { get; init; } = string.Empty;
    public decimal AdultPrice { get; init; }
    public decimal? ChildPrice { get; init; }
    public decimal? InfantPrice { get; init; }
    public decimal Subtotal { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}
