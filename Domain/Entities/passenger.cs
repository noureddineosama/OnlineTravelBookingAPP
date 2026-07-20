#nullable disable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("status", Name = "IX_users_status")]
[Index("email", Name = "UQ_users_email", IsUnique = true)]
public partial class passenger : AuditableEntity
{
    // id, created_at, updated_at inherited from AuditableEntity

    public int role_id { get; set; }

    [Required]
    [StringLength(150)]
    public string name { get; set; }

    [Required]
    [StringLength(256)]
    public string email { get; set; }

    [StringLength(256)]
    public string password_hash { get; set; }

    [StringLength(30)]
    public string phone { get; set; }

    public int? location_id { get; set; }

    public bool is_email_verified { get; set; }

    public string refreshToken { get; set; }
    public DateTime? refresh_token_expiry { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string status { get; set; }

    [InverseProperty("passenger")]
    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();

    [InverseProperty("passenger")]
    public virtual ICollection<favorite> favorites { get; set; } = new List<favorite>();

    [ForeignKey("location_id")]
    [InverseProperty("passengers")]
    public virtual location location { get; set; }

    [InverseProperty("passenger")]
    public virtual ICollection<review> reviews { get; set; } = new List<review>();

    [ForeignKey("role_id")]
    [InverseProperty("passengers")]
    public virtual role role { get; set; }
}
