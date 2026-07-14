namespace Application.Features.TourBookings.Requests;

/// <summary>Body for PUT /api/tour-bookings/{bookingId}/cancel</summary>
public sealed record CancelTourBookingRequest(string Reason = "No longer needed");
