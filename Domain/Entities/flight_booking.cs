#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("booking_id", Name = "UQ_flight_bookings_booking", IsUnique = true)]
public partial class flight_booking
{
    [Key]
    public long id { get; set; }

    public long booking_id { get; set; }

    public long flight_id { get; set; }

    public long? return_flight_id { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string trip_type { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal price { get; set; }

    [ForeignKey("booking_id")]
    [InverseProperty("flight_booking")]
    public virtual booking booking { get; set; }

    [ForeignKey("flight_id")]
    [InverseProperty("flight_bookingflights")]
    public virtual flight flight { get; set; }

    [InverseProperty("flight_booking")]
    public virtual ICollection<flight_booking_passenger> flight_booking_passengers { get; set; } = new List<flight_booking_passenger>();

    [ForeignKey("return_flight_id")]
    [InverseProperty("flight_bookingreturn_flights")]
    public virtual flight return_flight { get; set; }
}
