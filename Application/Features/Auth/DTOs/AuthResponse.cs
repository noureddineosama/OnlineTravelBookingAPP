namespace Application.Features.Auth.DTOs;

public sealed record AuthResponse(
    string Token,
    string RefreshToken,
    string Email,
    string Name
);
