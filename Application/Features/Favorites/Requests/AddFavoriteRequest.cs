using Domain.Enums;

namespace Application.Features.Favorites.Requests;

/// <summary>HTTP request body for POST /api/favorites</summary>
public sealed class AddFavoriteRequest
{
    public FavoriteCategory Category { get; init; }
    public long             ItemId   { get; init; }
}
