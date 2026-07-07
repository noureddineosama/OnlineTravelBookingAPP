#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class tour_price_tier
{
    [Key]
    public long id { get; set; }

    public long tour_id { get; set; }

    [Required]
    [StringLength(100)]
    public string name { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal adult_price { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? child_price { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? infant_price { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string currency { get; set; }

    [ForeignKey("tour_id")]
    [InverseProperty("tour_price_tiers")]
    public virtual tour tour { get; set; }

    [InverseProperty("price_tier")]
    public virtual ICollection<tour_schedule> tour_schedules { get; set; } = new List<tour_schedule>();
}
