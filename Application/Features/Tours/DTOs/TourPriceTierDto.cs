namespace Application.Features.Tours.DTOs;

public sealed class TourPriceTierDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal AdultPrice { get; init; }
    public decimal? ChildPrice { get; init; }
    public decimal? InfantPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
}
