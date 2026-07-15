using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class GetHotelRoomsResponseDTO 
    {
        public long RoomId { get; set; }

        public string Name { get; set; }

        public decimal PricePerNight { get; set; }

        public string BedType { get; set; }

        public bool Refundable { get; set; }

        public string CoverImage { get; set; }
    }
}
