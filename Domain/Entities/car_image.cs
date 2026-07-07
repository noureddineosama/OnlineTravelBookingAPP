#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class car_image
{
    [Key]
    public long id { get; set; }

    public long car_id { get; set; }

    [Required]
    [StringLength(500)]
    public string url { get; set; }

    public int sort_order { get; set; }

    [ForeignKey("car_id")]
    [InverseProperty("car_images")]
    public virtual car car { get; set; }
}
