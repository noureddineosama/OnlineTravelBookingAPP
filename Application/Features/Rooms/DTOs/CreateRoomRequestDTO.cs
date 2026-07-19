using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public  class CreateRoomRequestDTO
    {

        public string Name { get; set; }

        public string BedType { get; set; }

        public int OccupancyAdults { get; set; }

        public int OccupancyChildren { get; set; }

        public decimal PricePerNight { get; set; }

        public bool Refundable { get; set; }
    }
}
