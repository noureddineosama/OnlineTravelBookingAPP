using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelAvailability.DTOs
{
    public class CheckRoomAvailabilityRequestDTO
    {
        public long room_id { get; set; }

        public DateOnly check_in_date { get; set; }

        public DateOnly check_out_date { get; set; }
    }
}
