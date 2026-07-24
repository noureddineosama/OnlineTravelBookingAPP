#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("user_id", "status", Name = "IX_bookings_user_status")]
[Index("booking_number", Name = "UQ_bookings_number", IsUnique = true)]
public partial class booking : AuditableEntity
{
    // id, created_at, updated_at inherited from AuditableEntity

    [Required]
    [StringLength(30)]
    public string booking_number { get; set; }

    public long user_id { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string category { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string? status { get; set; }
    public bool? IsCancelled { get; set; } = false;

    // Cancellation metadata
    public DateTime? cancelled_at { get; set; }
    public CancellationReasonType? cancellation_reason_type { get; set; }
    [StringLength(500)]
    public string cancellation_reason_details { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal subtotal { get; set; } //. is the same like totalPrice 

    [Column(TypeName = "decimal(10, 2)")]
    public decimal discount_amount { get; set; } //. not important 

    [Column(TypeName = "decimal(10, 2)")]
    public decimal total_price { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string currency { get; set; }

    [Required]
    [StringLength(15)]
    [Unicode(false)]
    public string payment_status { get; set; }

    [InverseProperty("booking")]
    public virtual car_booking car_booking { get; set; }

    [InverseProperty("booking")]
    public virtual flight_booking flight_booking { get; set; }

    [InverseProperty("booking")]
    public virtual hotel_booking hotel_booking { get; set; }

    [InverseProperty("booking")]
    public virtual ICollection<payment> payments { get; set; } = new List<payment>();

    [InverseProperty("booking")]
    public virtual ICollection<review> reviews { get; set; } = new List<review>();

    [InverseProperty("booking")]
    public virtual tour_booking tour_booking { get; set; }

    [ForeignKey("user_id")]
    [InverseProperty("bookings")]
    public virtual passenger passenger { get; set; }
}
