namespace Application.Features.Auth.DTOs;

/// <summary>
/// Lightweight internal record cached by HybridCache for passenger auth lookups.
/// Holds only the fields needed by Login — never caches EF-tracked entities.
/// </summary>
internal sealed record CachedPassengerProfile(
    long Id,
    string Email,
    string Name,
    string PasswordHash,
    string? RoleName
);
