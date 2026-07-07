#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class room_image
{
    [Key]
    public long id { get; set; }

    public long room_id { get; set; }

    [Required]
    [StringLength(500)]
    public string url { get; set; }

    public int sort_order { get; set; }

    [ForeignKey("room_id")]
    [InverseProperty("room_images")]
    public virtual room room { get; set; }
}
