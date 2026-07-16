using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class RoomExtrasResponseDTO
    {
        public long roomId { get; set; }
        public List<UpdateRoomExtraResponseDTO> extras { get; set; }
    }
}
