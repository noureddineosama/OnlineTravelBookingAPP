using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelAvailability.DTOs
{
    public class CheckRoomAvailabilityRequestDTO
    {
        public long RoomId { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public int Quantity { get; set; }
    }
}
