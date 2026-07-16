using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class GetHotelsResponseDTO
    {
        public long HotelId { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string City { get; set; }

        public byte? StarRating { get; set; }

        public string MainImageUrl { get; set; }

        public string Status { get; set; }

        public int RoomsCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
