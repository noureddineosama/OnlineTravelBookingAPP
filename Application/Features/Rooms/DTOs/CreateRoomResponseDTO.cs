using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class CreateRoomResponseDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal PricePerNight { get; set; }

        public string Status { get; set; }
    }
}
