#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("name", Name = "UQ_car_categories_name", IsUnique = true)]
public partial class car_category
{
    [Key]
    public int id { get; set; }

    [Required]
    [StringLength(50)]
    public string name { get; set; }

    [InverseProperty("car_category")]
    public virtual ICollection<car> cars { get; set; } = new List<car>();
}
