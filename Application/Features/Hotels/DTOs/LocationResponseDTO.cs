using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class LocationResponseDTO
    {
        public int Id { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Address { get; set; }
    }
}
