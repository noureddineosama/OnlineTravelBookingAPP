using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.DTOs
{
    public class HotelBookingDetailsResponseDTO
    {
        public long BookingId { get; set; }

        public string HotelName { get; set; }

        public string RoomName { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public decimal PricePerNight { get; set; }

        public decimal? TotalPrice { get; set; }

        public int Adults { get; set; }

        public int Children { get; set; }

        public string Status { get; set; }
    }
}
