using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class DeleteHotelResponseDTO
    {
        public long HotelId { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }
    }
}
