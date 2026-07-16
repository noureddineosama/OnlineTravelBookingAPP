namespace Domain.Enums;

/// <summary>
/// Centralises the DB string representation of <see cref="FavoriteCategory"/>.
/// The category column stores lowercase strings: "tour", "hotel", "flight", "car".
/// Using this extension everywhere means a rename only ever breaks one place.
/// </summary>
public static class FavoriteCategoryExtensions
{
    public static string ToDbString(this FavoriteCategory category)
        => category.ToString().ToLower();
}
