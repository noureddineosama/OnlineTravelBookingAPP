#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("slug", Name = "UQ_tours_slug", IsUnique = true)]
public partial class tour : AuditableEntity
{
    // id, created_at, updated_at inherited from AuditableEntity

    [Required]
    [StringLength(200)]
    public string title { get; set; }

    [Required]
    [StringLength(200)]
    public string slug { get; set; }

    [StringLength(500)]
    public string summary { get; set; }

    public string full_description { get; set; }

    [StringLength(500)]
    public string main_image_url { get; set; }

    public int? duration_days { get; set; }

    public int? location_id { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string difficulty { get; set; }

    public TourStatus status { get; set; }

    // Soft delete metadata
    public bool is_deleted { get; set; } = false;
    public DateTime? deleted_at { get; set; }
    public long? deleted_by { get; set; }

    // Cancellation metadata
    public DateTime? cancelled_at { get; set; }
    public long? cancelled_by { get; set; }
    public CancellationReasonType? cancellation_reason_type { get; set; }
    [StringLength(500)]
    public string cancellation_reason_details { get; set; }

    [ForeignKey("location_id")]
    [InverseProperty("tours")]
    public virtual location location { get; set; }

    [InverseProperty("tour")]
    public virtual ICollection<tour_image> tour_images { get; set; } = new List<tour_image>();

    [InverseProperty("tour")]
    public virtual ICollection<tour_inclusion> tour_inclusions { get; set; } = new List<tour_inclusion>();

    [InverseProperty("tour")]
    public virtual ICollection<tour_price_tier> tour_price_tiers { get; set; } = new List<tour_price_tier>();

    [InverseProperty("tour")]
    public virtual ICollection<tour_schedule> tour_schedules { get; set; } = new List<tour_schedule>();

    /// <summary>
    /// Soft deletes the tour and marks it as cancelled.
    /// </summary>
    public void SoftDelete(long adminId, string reasonDetails, CancellationReasonType reasonType = CancellationReasonType.AdminCancelled)
    {
        if (is_deleted) return;

        is_deleted = true;
        deleted_at = DateTime.UtcNow;
        deleted_by = adminId;
        
        status = TourStatus.Cancelled;
        cancelled_at = DateTime.UtcNow;
        cancelled_by = adminId;
        cancellation_reason_type = reasonType;
        cancellation_reason_details = reasonDetails;
    }
}
