#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class room : CreatedAtEntity
{
    public long hotel_id { get; set; }

    [Required]
    [StringLength(100)]
    public string name { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string bed_type { get; set; }

    public int occupancy_adults { get; set; }

    public int occupancy_children { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal price_per_night { get; set; }

    public bool refundable { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string status { get; set; } = "Active";

    [ForeignKey("hotel_id")]
    [InverseProperty("rooms")]
    public virtual hotel hotel { get; set; }

    [InverseProperty("room")]
    public virtual ICollection<hotel_booking> hotel_bookings { get; set; } = new List<hotel_booking>();

    [InverseProperty("room")]
    public virtual ICollection<room_availability> room_availabilities { get; set; } = new List<room_availability>();

    [InverseProperty("room")]
    public virtual ICollection<room_extra> room_extras { get; set; } = new List<room_extra>();

    [InverseProperty("room")]
    public virtual ICollection<room_image> room_images { get; set; } = new List<room_image>();
}
