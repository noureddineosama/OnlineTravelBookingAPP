using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class UpdateHotelResponseDTO
    {
        public long HotelId { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public byte? StarRating { get; set; }

        public string Status { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
