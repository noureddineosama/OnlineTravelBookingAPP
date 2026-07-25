using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Application.Features.Booking.DTOs
{
    public class BookingDetailsResponseDTO
    {
        public long BookingId { get; set; }

        public string Category { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TotalPrice { get; set; }

        public string Currency { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public object Details { get; set; }
    }
}
