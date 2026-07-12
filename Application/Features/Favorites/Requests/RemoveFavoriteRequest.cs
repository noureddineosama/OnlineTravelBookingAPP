using Domain.Enums;

namespace Application.Features.Favorites.Requests;

/// <summary>HTTP request body for DELETE /api/favorites</summary>
public sealed class RemoveFavoriteRequest
{
    public long             UserId   { get; init; }
    public FavoriteCategory Category { get; init; }
    public long             ItemId   { get; init; }
}
