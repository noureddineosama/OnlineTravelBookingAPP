using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class UpdateRoomExtraResponseDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}
