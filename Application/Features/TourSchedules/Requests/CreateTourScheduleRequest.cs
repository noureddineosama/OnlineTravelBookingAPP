namespace Application.Features.TourSchedules.Requests;

public sealed record CreateTourScheduleRequest(
    long PriceTierId,
    DateTime StartDate,
    DateTime? EndDate,
    int Capacity
);
