#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("category", "item_id", Name = "IX_reviews_category_item")]
[Index("user_id", Name = "IX_reviews_user")]
public partial class review
{
    [Key]
    public long id { get; set; }

    public long user_id { get; set; }

    public long? booking_id { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string category { get; set; }

    public long item_id { get; set; }

    public byte rating { get; set; }

    [StringLength(150)]
    public string title { get; set; }

    public string body { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string status { get; set; }

    public DateTime created_at { get; set; }

    [ForeignKey("booking_id")]
    [InverseProperty("reviews")]
    public virtual booking booking { get; set; }

    [ForeignKey("user_id")]
    [InverseProperty("reviews")]
    public virtual passenger passenger { get; set; }
}
