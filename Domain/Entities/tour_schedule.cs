#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("tour_id", "start_date", Name = "IX_tour_schedules_tour_start", IsUnique = true)]
public partial class tour_schedule
{
    [Key]
    public long id { get; set; }

    public long tour_id { get; set; }

    public long price_tier_id { get; set; }

    public DateTime start_date { get; set; }

    public DateTime? end_date { get; set; }

    public int capacity { get; set; }

    public int available_slots { get; set; }

    public bool is_cancelled { get; set; }

    public DateTime? cancelled_at { get; set; }

    [StringLength(500)]
    public string? cancellation_reason { get; set; }

    [ForeignKey("price_tier_id")]
    [InverseProperty("tour_schedules")]
    public virtual tour_price_tier price_tier { get; set; }

    [ForeignKey("tour_id")]
    [InverseProperty("tour_schedules")]
    public virtual tour tour { get; set; }

    [InverseProperty("tour_schedule")]
    public virtual ICollection<tour_booking> tour_bookings { get; set; } = new List<tour_booking>();
}
