using Domain.Enums;

namespace Application.Features.Favorites.DTOs;

/// <summary>
/// UI-ready DTO returned by GetMyFavorites.
/// Contains all the data the frontend needs to render a favourite card
/// for any category (Tour, Hotel, Flight, Car) without making extra requests.
///
/// Fields that do not apply to a category are null.
/// Example: Location is null for Flights. Subtitle is null for Tours.
/// </summary>
public sealed class FavoriteDto
{
    // ── Core ──────────────────────────────────────────────────────────────────

    public long             FavoriteId    { get; init; }
    public long             UserId        { get; init; }
    public FavoriteCategory Category      { get; init; }

    /// <summary>Human-readable category label: "Tour", "Hotel", "Flight", "Car".</summary>
    public string           CategoryLabel { get; init; } = string.Empty;

    public long             ItemId        { get; init; }
    public DateTime         AddedAt       { get; init; }

    // ── Display fields (UI-ready) ─────────────────────────────────────────────

    /// <summary>Tour title / Hotel name / Flight route / Car brand+model.</summary>
    public string           Title         { get; init; } = string.Empty;

    /// <summary>Secondary line: tour summary / hotel description / carrier+cabin / car transmission.</summary>
    public string?          Subtitle      { get; init; }

    public string?          ImageUrl      { get; init; }

    /// <summary>Starting price: lowest tier (Tour), cheapest room/night (Hotel), base price (Flight), lowest tier/hour (Car).</summary>
    public decimal?         Price         { get; init; }
    public string?          Currency      { get; init; }

    /// <summary>Average approved review rating. Null if no reviews exist.</summary>
    public double?          Rating        { get; init; }

    /// <summary>City + Country string. Null for Flights (route used instead).</summary>
    public string?          Location      { get; init; }

    /// <summary>Category-specific badge: "7 Days" / "5★" / "CAI→LHR" / "SUV".</summary>
    public string?          BadgeText     { get; init; }
}
