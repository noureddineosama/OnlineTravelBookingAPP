#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("booking_id", Name = "UQ_hotel_bookings_booking", IsUnique = true)]
public partial class hotel_booking
{
    [Key]
    public long id { get; set; }

    public long booking_id { get; set; }

    public long room_id { get; set; }

    public DateOnly check_in_date { get; set; }

    public DateOnly check_out_date { get; set; }

    public int quantity { get; set; }

    public int guests_adults { get; set; }

    public int guests_children { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal price_per_night { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? TotalPrice { get; set; }

    [ForeignKey("booking_id")]
    [InverseProperty("hotel_booking")]
    public virtual booking booking { get; set; }

    [ForeignKey("room_id")]
    [InverseProperty("hotel_bookings")]
    public virtual room room { get; set; }
}
