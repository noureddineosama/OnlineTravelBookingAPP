using Domain.Enums;
using System;

namespace Application.Features.HotelBooking.DTOs
{
    public class CreateHotelBookingResponseDTO
    {
        public long BookingId { get; set; }

        public long HotelBookingId { get; set; }

        public string HotelName { get; set; }

        public string RoomName { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public decimal PricePerNight { get; set; }

        public decimal? TotalPrice { get; set; }

        public BookingStatus Status { get; set; }
    }
}
