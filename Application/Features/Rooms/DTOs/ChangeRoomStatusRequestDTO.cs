using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class ChangeRoomStatusRequestDTO
    {
        public RoomStatus Status { get; set; }
    }
}
