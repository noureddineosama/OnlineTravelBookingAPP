using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.DTOs
{
    public class CreateHotelBookingRequestDTO
    {
        public long room_id { get; set; }

        public DateOnly check_in_date { get; set; }

        public DateOnly check_out_date { get; set; }

        public int guests_adults { get; set; }

        public int guests_children { get; set; }
    }
}
