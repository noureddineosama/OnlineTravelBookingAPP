namespace Application.Features.FavouriteTours.DTOs;

/// <summary>
/// Output DTO for a favourited tour. Includes tour details and the starting price.
/// </summary>
public sealed class FavouriteTourResponse
{
    public long FavouriteId { get; init; }
    public long TourId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? MainImageUrl { get; init; }
    public int? DurationDays { get; init; }
    public string? LocationCity { get; init; }
    public string? LocationCountry { get; init; }
    public decimal? StartingFromPrice { get; init; }
    public string? Currency { get; init; }
    public DateTime AddedAt { get; init; }
}
