using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelAvailability.DTOs
{
    public class CheckRoomAvailabilityResponseDTO
    {
        public bool IsAvailable { get; set; }

        public string Message { get; set; }
    }
}
