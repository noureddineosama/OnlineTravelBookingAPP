#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class flight_booking_passenger
{
    [Key]
    public long id { get; set; }

    public long flight_booking_id { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string title { get; set; }

    [Required]
    [StringLength(100)]
    public string first_name { get; set; }

    [Required]
    [StringLength(100)]
    public string last_name { get; set; }

    [StringLength(30)]
    public string passport_number { get; set; }

    [ForeignKey("flight_booking_id")]
    [InverseProperty("flight_booking_passengers")]
    public virtual flight_booking flight_booking { get; set; }
}
