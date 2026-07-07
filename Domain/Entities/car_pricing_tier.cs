#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class car_pricing_tier
{
    [Key]
    public long id { get; set; }

    public long car_id { get; set; }

    public int from_hours { get; set; }

    public int? to_hours { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal price_per_hour { get; set; }

    [ForeignKey("car_id")]
    [InverseProperty("car_pricing_tiers")]
    public virtual car car { get; set; }
}
