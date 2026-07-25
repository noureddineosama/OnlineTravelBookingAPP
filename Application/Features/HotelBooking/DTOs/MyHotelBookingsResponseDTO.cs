using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.DTOs
{
    public class MyHotelBookingsResponseDTO
    {
        public long BookingId { get; set; }

        public string HotelName { get; set; }

        public string MainImage { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }
    }
}
