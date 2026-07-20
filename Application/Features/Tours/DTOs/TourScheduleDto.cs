namespace Application.Features.Tours.DTOs;

public sealed class TourScheduleDto
{
    public long Id { get; init; }
    public long PriceTierId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Capacity { get; init; }
    public int AvailableSlots { get; init; }
}
