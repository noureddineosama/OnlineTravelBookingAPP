using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CarBookings.DTOs
{
    public class CreateCarBookingRequestDTO
    {
        public long car_id { get; set; }

        public int pickup_location_id { get; set; }

        public int dropoff_location_id { get; set; }

        public DateTime pickup_at { get; set; }

        public DateTime dropoff_at { get; set; }

        public string? driver_name { get; set; }

        public List<CarExtraItemDTO> extras { get; set; } = new();
    }

    public class CarExtraItemDTO
    {
        public int extra_id { get; set; }

        public int quantity { get; set; }
    }
}
