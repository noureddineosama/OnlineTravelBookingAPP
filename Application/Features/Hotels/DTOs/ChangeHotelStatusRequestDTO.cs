using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class ChangeHotelStatusRequestDTO
    {
        public long HotelId { get; set; }

        public hotel_Status Status { get; set; }

    }
}
