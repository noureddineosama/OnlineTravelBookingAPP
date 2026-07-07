#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class tour_image
{
    [Key]
    public long id { get; set; }

    public long tour_id { get; set; }

    [Required]
    [StringLength(500)]
    public string url { get; set; }

    public int sort_order { get; set; }

    [ForeignKey("tour_id")]
    [InverseProperty("tour_images")]
    public virtual tour tour { get; set; }
}
