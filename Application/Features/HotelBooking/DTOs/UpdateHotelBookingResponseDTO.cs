using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.DTOs
{
    public class UpdateHotelBookingResponseDTO 
    {
        public long HotelBookingId { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public int NumberOfNights { get; set; }

        public int Adults { get; set; }

        public int Children { get; set; }

        public decimal PricePerNight { get; set; }

        public decimal SubTotal { get; set; }

        public string BookingStatus { get; set; }

        public string Message { get; set; }
    }
}
