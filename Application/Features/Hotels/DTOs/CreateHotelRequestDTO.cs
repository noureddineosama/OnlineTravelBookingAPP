using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class CreateHotelRequestDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int LocationId { get; set; }

        public byte? StarRating { get; set; }

        public TimeOnly? CheckInTime { get; set; }

        public TimeOnly? CheckOutTime { get; set; }

        public hotel_Status Status { get; set; }
    }
}
