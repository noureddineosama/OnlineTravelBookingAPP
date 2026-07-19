using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CarBookings.DTOs
{
    public class MyCarBookingsResponseDTO
    {
        public long BookingId { get; set; }

        public string BookingNumber { get; set; }

        public string CarModel { get; set; }

        public string CarBrand { get; set; }

        public string MainImageUrl { get; set; }

        public DateTime PickupAt { get; set; }

        public DateTime DropoffAt { get; set; }

        public string PickupLocation { get; set; }

        public string DropoffLocation { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }
    }
}
