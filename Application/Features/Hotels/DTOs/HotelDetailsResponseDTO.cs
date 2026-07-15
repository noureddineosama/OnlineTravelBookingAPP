using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class HotelDetailsResponseDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public byte? StarRating { get; set; }

        public string MainImageUrl { get; set; }

        public string Status { get; set; }

        public TimeOnly? CheckInTime { get; set; }

        public TimeOnly? CheckOutTime { get; set; }

        public LocationResponseDTO Location { get; set; }

        public List<HotelImageResponseDTO> Images { get; set; }

        public List<RoomResponsedTO> Rooms { get; set; }
    }
}
