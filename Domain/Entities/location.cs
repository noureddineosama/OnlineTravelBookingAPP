#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class location
{
    [Key]
    public int id { get; set; }

    [Required]
    [StringLength(100)]
    public string country { get; set; }

    [Required]
    [StringLength(100)]
    public string city { get; set; }

    [StringLength(255)]
    public string address_line { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? latitude { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? longitude { get; set; }

    [InverseProperty("dropoff_location")]
    public virtual ICollection<car_booking> car_bookingdropoff_locations { get; set; } = new List<car_booking>();

    [InverseProperty("pickup_location")]
    public virtual ICollection<car_booking> car_bookingpickup_locations { get; set; } = new List<car_booking>();

    [InverseProperty("dropoff_location")]
    public virtual ICollection<car> cardropoff_locations { get; set; } = new List<car>();

    [InverseProperty("pickup_location")]
    public virtual ICollection<car> carpickup_locations { get; set; } = new List<car>();

    [InverseProperty("location")]
    public virtual ICollection<hotel> hotels { get; set; } = new List<hotel>();

    [InverseProperty("location")]
    public virtual ICollection<tour> tours { get; set; } = new List<tour>();

    [InverseProperty("location")]
    public virtual ICollection<passenger> passengers { get; set; } = new List<passenger>();
}
