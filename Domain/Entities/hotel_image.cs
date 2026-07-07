#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class hotel_image
{
    [Key]
    public long id { get; set; }

    public long hotel_id { get; set; }

    [Required]
    [StringLength(500)]
    public string url { get; set; }

    public int sort_order { get; set; }

    [ForeignKey("hotel_id")]
    [InverseProperty("hotel_images")]
    public virtual hotel hotel { get; set; }
}
