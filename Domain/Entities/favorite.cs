#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("user_id", "category", "item_id", Name = "UQ_favorites", IsUnique = true)]
public partial class favorite
{
    [Key]
    public long id { get; set; }

    public long user_id { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string category { get; set; }

    public long item_id { get; set; }

    public DateTime added_at { get; set; }

    [ForeignKey("user_id")]
    [InverseProperty("favorites")]
    public virtual passenger passenger { get; set; }
}
