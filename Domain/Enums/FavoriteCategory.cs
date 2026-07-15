namespace Domain.Enums;

/// <summary>
/// Represents the type of item saved as a favourite.
/// Stored as a lowercase string in the database (favorite.category column).
/// Adding a new category only requires a new enum value here.
/// </summary>
public enum FavoriteCategory
{
    Tour   = 1,
    Hotel  = 2,
    Flight = 3,
    Car    = 4
}
