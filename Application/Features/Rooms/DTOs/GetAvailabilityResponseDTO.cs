using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class GetAvailabilityResponseDTO
    {
        public List<AvailabilityDayDTO> availabilities { get; set; }
    }
}
