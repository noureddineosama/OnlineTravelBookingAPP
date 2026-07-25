using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.DTOs
{
    public class CancelBookingResponseDTO
    {
        public long BookingId { get; set; }

        public string Status { get; set; }

        public DateTime CancelledAt { get; set; }

        public decimal RefundAmount { get; set; }
    }
}
