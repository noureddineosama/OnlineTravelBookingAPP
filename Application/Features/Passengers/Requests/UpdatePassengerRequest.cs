namespace Application.Features.Passengers.Requests;

/// <summary>Body for PUT /api/passengers/{id}</summary>
public sealed record UpdatePassengerRequest(
    string? Name   = null,
    string? Phone  = null,
    string? Status = null);
