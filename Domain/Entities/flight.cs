#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("origin_airport_code", "destination_airport_code", "departure_at_utc", Name = "IX_flights_route_departure")]
public partial class flight : CreatedAtEntity
{
    // id, created_at inherited from CreatedAtEntity

    [Required]
    [StringLength(20)]
    public string flight_number { get; set; }

    [Required]
    [StringLength(100)]
    public string carrier_name { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string origin_airport_code { get; set; }

    [Required]
    [StringLength(100)]
    public string origin_city { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string destination_airport_code { get; set; }

    [Required]
    [StringLength(100)]
    public string destination_city { get; set; }

    public DateTime departure_at_utc { get; set; }

    public DateTime arrival_at_utc { get; set; }

    public int? duration_minutes { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string cabin_class { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal base_price { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string currency { get; set; }

    public int seats_available { get; set; }

    [Required]
    [StringLength(15)]
    [Unicode(false)]
    public string status { get; set; }

    [InverseProperty("flight")]
    public virtual ICollection<flight_booking> flight_bookingflights { get; set; } = new List<flight_booking>();

    [InverseProperty("return_flight")]
    public virtual ICollection<flight_booking> flight_bookingreturn_flights { get; set; } = new List<flight_booking>();
}
