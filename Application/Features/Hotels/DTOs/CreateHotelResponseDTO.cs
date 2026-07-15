using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class CreateHotelResponseDTO
    {
        public long HotelId { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
