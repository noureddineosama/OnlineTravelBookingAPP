namespace Application.Features.Passengers.Requests;

/// <summary>Body for POST /api/passengers</summary>
public sealed record CreatePassengerRequest(
    string  Name,
    string  Email,
    string? Phone  = null,
    int     RoleId = 1);
