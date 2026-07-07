#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("booking_id", Name = "IX_payments_booking")]
[Index("transaction_id", Name = "IX_payments_transaction")]
public partial class payment
{
    [Key]
    public long id { get; set; }

    public long booking_id { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal amount { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string currency { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string gateway { get; set; }

    [Required]
    [StringLength(15)]
    [Unicode(false)]
    public string status { get; set; }

    [StringLength(100)]
    public string transaction_id { get; set; }

    public DateTime created_at { get; set; }

    [ForeignKey("booking_id")]
    [InverseProperty("payments")]
    public virtual booking booking { get; set; }
}
