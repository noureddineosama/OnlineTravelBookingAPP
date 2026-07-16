using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class RoomDetailsResponseDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BedType { get; set; }

        public decimal PricePerNight { get; set; }

        public bool Refundable { get; set; }

        public string Status { get; set; }

        public int Adults { get; set; }

        public int Children { get; set; }

        public List<CreateRoomExtraResponseDTO> Extras { get; set; }

        public List<RoomImageDTO> Images { get; set; }
    }
}
