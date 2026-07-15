using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    //. will be sent in the query string of the request, so we can use nullable types for optional parameters
    //.       ( that's better for front-end team, they can omit the parameters they don't want to send)
    public class SearchRequestDTO
    {
        public string? City { get; set; }

        public TimeOnly? CheckInDate { get; set; }

        public TimeOnly? CheckOutDate { get; set; }
        public long? location_id { get; set; }

        public int Adults { get; set; }

        public int Children { get; set; }

        public byte? StarRating { get; set; }

        public int PageNumber { get; set; } 

        public int PageSize { get; set; }
    }
}
