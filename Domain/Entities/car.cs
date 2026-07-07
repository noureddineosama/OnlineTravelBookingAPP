#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class car : CreatedAtEntity
{
    // id, created_at inherited from CreatedAtEntity

    public int brand_id { get; set; }

    public int car_category_id { get; set; }

    [Required]
    [StringLength(100)]
    public string model { get; set; }

    public int? year { get; set; }

    public int seats_count { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string transmission { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string fuel_type { get; set; }

    public int? pickup_location_id { get; set; }

    public int? dropoff_location_id { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal price_per_day { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string status { get; set; }

    [ForeignKey("brand_id")]
    [InverseProperty("cars")]
    public virtual car_brand brand { get; set; }

    [InverseProperty("car")]
    public virtual ICollection<car_booking> car_bookings { get; set; } = new List<car_booking>();

    [ForeignKey("car_category_id")]
    [InverseProperty("cars")]
    public virtual car_category car_category { get; set; }

    [InverseProperty("car")]
    public virtual ICollection<car_image> car_images { get; set; } = new List<car_image>();

    [InverseProperty("car")]
    public virtual ICollection<car_pricing_tier> car_pricing_tiers { get; set; } = new List<car_pricing_tier>();

    [ForeignKey("dropoff_location_id")]
    [InverseProperty("cardropoff_locations")]
    public virtual location dropoff_location { get; set; }

    [ForeignKey("pickup_location_id")]
    [InverseProperty("carpickup_locations")]
    public virtual location pickup_location { get; set; }
}
