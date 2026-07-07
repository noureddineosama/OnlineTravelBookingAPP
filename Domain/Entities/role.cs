#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("name", Name = "UQ_roles_name", IsUnique = true)]
public partial class role
{
    [Key]
    public int id { get; set; }

    [Required]
    [StringLength(50)]
    public string name { get; set; }

    public DateTime created_at { get; set; }

    [InverseProperty("role")]
    public virtual ICollection<passenger> passengers { get; set; } = new List<passenger>();
}
