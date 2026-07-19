namespace Application.Features.Tours.DTOs;

public sealed record CreateTourPriceTierDto(
    string Name,
    decimal AdultPrice,
    decimal? ChildPrice,
    decimal? InfantPrice,
    string Currency
);

public sealed record CreateTourScheduleDto(
    DateTime StartDate,
    DateTime EndDate,
    int AvailableSlots
);

public sealed record CreateTourResponse(
    long TourId,
    long? ScheduleId,
    long? PriceTierId
);
