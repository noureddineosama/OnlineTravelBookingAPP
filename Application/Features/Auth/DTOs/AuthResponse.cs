namespace Application.Features.Auth.DTOs;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string Email,
    string Name,
    string Role
);
