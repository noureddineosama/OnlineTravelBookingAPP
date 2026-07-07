#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("booking_id", Name = "UQ_tour_bookings_booking", IsUnique = true)]
public partial class tour_booking
{
    [Key]
    public long id { get; set; }

    public long booking_id { get; set; }

    public long tour_schedule_id { get; set; }

    public int adults_count { get; set; }

    public int children_count { get; set; }

    public int infants_count { get; set; }

    [ForeignKey("booking_id")]
    [InverseProperty("tour_booking")]
    public virtual booking booking { get; set; }

    [ForeignKey("tour_schedule_id")]
    [InverseProperty("tour_bookings")]
    public virtual tour_schedule tour_schedule { get; set; }
}
