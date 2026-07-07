#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class room_extra
{
    [Key]
    public long id { get; set; }

    public long room_id { get; set; }

    [Required]
    [StringLength(100)]
    public string name { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal price { get; set; }

    [ForeignKey("room_id")]
    [InverseProperty("room_extras")]
    public virtual room room { get; set; }
}
