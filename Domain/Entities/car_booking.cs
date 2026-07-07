#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("booking_id", Name = "UQ_car_bookings_booking", IsUnique = true)]
public partial class car_booking
{
    [Key]
    public long id { get; set; }

    public long booking_id { get; set; }

    public long car_id { get; set; }

    public int pickup_location_id { get; set; }

    public int dropoff_location_id { get; set; }

    public DateTime pickup_at { get; set; }

    public DateTime dropoff_at { get; set; }

    [StringLength(150)]
    public string driver_name { get; set; }

    [ForeignKey("booking_id")]
    [InverseProperty("car_booking")]
    public virtual booking booking { get; set; }

    [ForeignKey("car_id")]
    [InverseProperty("car_bookings")]
    public virtual car car { get; set; }

    [InverseProperty("car_booking")]
    public virtual ICollection<car_booking_extra> car_booking_extras { get; set; } = new List<car_booking_extra>();

    [ForeignKey("dropoff_location_id")]
    [InverseProperty("car_bookingdropoff_locations")]
    public virtual location dropoff_location { get; set; }

    [ForeignKey("pickup_location_id")]
    [InverseProperty("car_bookingpickup_locations")]
    public virtual location pickup_location { get; set; }
}
