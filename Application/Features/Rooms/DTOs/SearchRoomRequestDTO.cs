using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public  class SearchRoomRequestDTO
    {
        public int? Adults { get; set; }

        public int? Children { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public bool? Refundable { get; set; }

        public string BedType { get; set; }
    }
}
