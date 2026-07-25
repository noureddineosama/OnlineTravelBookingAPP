using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.DTOs
{
    public class GetBookingStatusResponseDTO
    {
        public long BookingId { get; set; }

        public string Status { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
