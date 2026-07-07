#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class car_extra
{
    [Key]
    public int id { get; set; }

    [Required]
    [StringLength(100)]
    public string name { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal price { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string pricing_type { get; set; }

    [InverseProperty("car_extra")]
    public virtual ICollection<car_booking_extra> car_booking_extras { get; set; } = new List<car_booking_extra>();
}
