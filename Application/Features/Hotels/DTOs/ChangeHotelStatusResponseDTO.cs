using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class ChangeHotelStatusResponseDTO
    {
        public long HotelId { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public string Message { get; set; }
    }
}
