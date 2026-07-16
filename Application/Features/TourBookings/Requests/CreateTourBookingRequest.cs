namespace Application.Features.TourBookings.Requests;

/// <summary>Body for POST /api/tour-bookings</summary>
public sealed record CreateTourBookingRequest(
    long TourScheduleId,
    int  AdultsCount,
    int  ChildrenCount = 0,
    int  InfantsCount  = 0);
