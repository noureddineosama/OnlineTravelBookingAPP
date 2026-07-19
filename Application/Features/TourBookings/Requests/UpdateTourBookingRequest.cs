namespace Application.Features.TourBookings.Requests;

public sealed record UpdateTourBookingRequest(
    int AdultsCount,
    int ChildrenCount,
    int InfantsCount
);
