#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
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

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string status { get; set; }

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
}
