#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("code", Name = "UQ_coupons_code", IsUnique = true)]
public partial class coupon
{
    [Key]
    public int id { get; set; }

    [Required]
    [StringLength(50)]
    public string code { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string discount_type { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal discount_value { get; set; }

    public int? max_usage { get; set; }

    public int usage_count { get; set; }

    public DateTime start_date { get; set; }

    public DateTime end_date { get; set; }

    public bool is_active { get; set; }

    [InverseProperty("coupon")]
    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();
}
