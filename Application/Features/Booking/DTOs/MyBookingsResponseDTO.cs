using Domain.Enums;

namespace Application.Features.Booking.DTOs
{
    public class MyBookingsResponseDTO
    {
        public long BookingId { get; set; }

        public string Category { get; set; }

        public string Title { get; set; }     

        public decimal TotalPrice { get; set; }

        public string Currency { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
