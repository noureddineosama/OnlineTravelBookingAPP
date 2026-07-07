#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class tour_inclusion
{
    [Key]
    public long id { get; set; }

    public long tour_id { get; set; }

    [Required]
    [StringLength(300)]
    public string item_text { get; set; }

    public bool is_included { get; set; }

    [ForeignKey("tour_id")]
    [InverseProperty("tour_inclusions")]
    public virtual tour tour { get; set; }
}
