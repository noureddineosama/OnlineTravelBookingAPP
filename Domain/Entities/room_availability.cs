#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("room_availability")]
[Index("room_id", "date", Name = "UQ_room_availability", IsUnique = true)]
public partial class room_availability
{
    [Key]
    public long id { get; set; }

    public long room_id { get; set; }

    public DateOnly date { get; set; }

    public bool IsAvailable { get; set; } = true;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? price_override { get; set; }

    [ForeignKey("room_id")]
    [InverseProperty("room_availabilities")]
    public virtual room room { get; set; }
}
