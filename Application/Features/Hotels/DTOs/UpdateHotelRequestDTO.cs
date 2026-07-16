using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class UpdateHotelRequestDTO
    {
        public string name { get; set; }

        public string description { get; set; }

        public int location_id { get; set; }

        public byte? star_rating { get; set; }

        public TimeOnly? check_in_time { get; set; }

        public TimeOnly? check_out_time { get; set; }

        public hotel_Status Status { get; set; }
    }
}
