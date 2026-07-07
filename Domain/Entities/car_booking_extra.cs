#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class car_booking_extra
{
    [Key]
    public long id { get; set; }

    public long car_booking_id { get; set; }

    public int car_extra_id { get; set; }

    public int quantity { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal price { get; set; }

    [ForeignKey("car_booking_id")]
    [InverseProperty("car_booking_extras")]
    public virtual car_booking car_booking { get; set; }

    [ForeignKey("car_extra_id")]
    [InverseProperty("car_booking_extras")]
    public virtual car_extra car_extra { get; set; }
}
