namespace Application.Features.Auth.DTOs;

public sealed record AuthResponse(
    string Token,
    string Email,
    string Name
);
