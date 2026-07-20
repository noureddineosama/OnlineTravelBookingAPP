using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class RoomResponsedTO
    {
        public long RoomId { get; set; }

        public string RoomName { get; set; }

        public decimal PricePerNight { get; set; }

        public int MaxAdults { get; set; }

        public int MaxChildren { get; set; }

        public bool IsAvailable { get; set; }

        public int AvailableRooms { get; set; }

        public string MainImageUrl { get; set; }
    }
}
