using System.ComponentModel.DataAnnotations;

namespace Domain.Common;

/// <summary>
/// Root base for all domain entities with a long primary key.
/// All lookup/join/image/booking sub-entities with no timestamps inherit directly from here.
/// </summary>
public abstract class BaseEntity
{
    [Key]
    public long id { get; set; }
}

/// <summary>
/// Base for reference / lookup tables that use an int primary key.
/// Examples: car_brand, car_category, car_extra, location, coupon.
/// </summary>
public abstract class BaseIntEntity
{
    [Key]
    public int id { get; set; }
}

/// <summary>
/// Extends BaseEntity for entities that are only ever inserted (never updated).
/// Examples: car, flight, hotel, payment, review.
/// </summary>
public abstract class CreatedAtEntity : BaseEntity
{
    /// <summary>UTC timestamp set automatically by the DB on insert.</summary>
    public DateTime created_at { get; set; } 
    public DateTime updated_at { get; set; }
    public long CreatedBy { get; set; }
    public long UpdatedBy { get; set; }
    public DateTime DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
