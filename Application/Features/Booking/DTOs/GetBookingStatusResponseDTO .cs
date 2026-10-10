using Domain.Enums;

namespace Application.Features.Booking.DTOs
{
    public class GetBookingStatusResponseDTO
    {
        public long BookingId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
