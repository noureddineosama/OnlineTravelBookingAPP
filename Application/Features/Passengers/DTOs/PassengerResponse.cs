namespace Application.Features.Passengers.DTOs;

/// <summary>
/// Output DTO for passenger data. Never expose domain entities to the API.
/// </summary>
public sealed class PassengerResponse
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsEmailVerified { get; init; }
    public string? RoleName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
