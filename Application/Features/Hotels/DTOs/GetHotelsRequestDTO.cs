using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class GetHotelsRequestDTO
    {
        public int? LocationId { get; set; }

        public hotel_Status? Status { get; set; }

        public byte? StarRating { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
