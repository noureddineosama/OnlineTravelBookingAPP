#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("name", Name = "UQ_car_brands_name", IsUnique = true)]
public partial class car_brand
{
    [Key]
    public int id { get; set; }

    [Required]
    [StringLength(100)]
    public string name { get; set; }

    [InverseProperty("brand")]
    public virtual ICollection<car> cars { get; set; } = new List<car>();
}
