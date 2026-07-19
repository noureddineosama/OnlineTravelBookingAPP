namespace Domain.Common;

/// <summary>
/// Extends CreatedAtEntity for entities that track both creation and last-update time.
/// Examples: passenger, booking, tour.
/// </summary>
public abstract class AuditableEntity : CreatedAtEntity
{
    /// <summary>
    /// UTC timestamp updated every time the record is modified.
    /// Null until the entity receives its first update.
    /// </summary>
    public new DateTime? updated_at { get; set; }
}

/// <summary>
/// Extends BaseIntEntity with a created_at audit timestamp.
/// For reference tables (int PK) that need to track when they were created.
/// Example: role.
/// </summary>
public abstract class AuditableIntEntity : BaseIntEntity
{
    /// <summary>UTC timestamp set by the DB on insert.</summary>
    public DateTime created_at { get; set; }
}
