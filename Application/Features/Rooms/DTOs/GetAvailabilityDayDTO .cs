using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class AvailabilityDayDTO 
    {
        public DateOnly Date { get; set; }

        public int AvailableUnits { get; set; }

        public decimal? PriceOverride { get; set; }
    }
}
