namespace Application.Features.Favorites.DTOs;

/// <summary>
/// Returned by CheckFavorite query.
/// Tells the frontend whether the item is already favourited
/// and supplies the FavoriteId so removal needs no extra lookup.
/// </summary>
public sealed class CheckFavoriteDto
{
    public bool  IsFavorited { get; init; }
    public long? FavoriteId  { get; init; }
}
