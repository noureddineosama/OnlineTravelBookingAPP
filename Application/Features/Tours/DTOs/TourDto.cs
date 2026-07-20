namespace Application.Features.Tours.DTOs;

using Domain.Enums;

public sealed class TourDto
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? FullDescription { get; init; }
    public string? MainImageUrl { get; init; }
    public int? DurationDays { get; init; }
    public int? LocationId { get; init; }
    public string? Difficulty { get; init; }
    public TourStatus Status { get; init; }
    
    public List<TourPriceTierDto> PriceTiers { get; init; } = new();
    public List<TourScheduleDto> Schedules { get; init; } = new();
}
