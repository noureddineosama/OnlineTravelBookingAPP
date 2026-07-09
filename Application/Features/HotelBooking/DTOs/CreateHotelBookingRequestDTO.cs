using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.DTOs
{
    public class CreateHotelBookingRequestDTO
    {
        public long RoomId { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public int Quantity { get; set; }

        public int Adults { get; set; }

        public int Children { get; set; }
    }
}
