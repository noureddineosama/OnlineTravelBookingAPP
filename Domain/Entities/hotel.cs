#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("slug", Name = "UQ_hotels_slug", IsUnique = true)]
public partial class hotel : CreatedAtEntity
{
    // id, created_at inherited from CreatedAtEntity

    [Required]
    [StringLength(200)]
    public string name { get; set; }

    [Required]
    [StringLength(200)]
    public string slug { get; set; }

    public string description { get; set; }

    public int location_id { get; set; }

    [StringLength(500)]
    public string main_image_url { get; set; }

    public byte? star_rating { get; set; }

    public TimeOnly? check_in_time { get; set; }

    public TimeOnly? check_out_time { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string status { get; set; }

    [InverseProperty("hotel")]
    public virtual ICollection<hotel_image>? hotel_images { get; set; } = new List<hotel_image>();

    [ForeignKey("location_id")]
    [InverseProperty("hotels")]
    public virtual location location { get; set; }

    [InverseProperty("hotel")]
    public virtual ICollection<room>? rooms { get; set; } = new List<room>();
}
