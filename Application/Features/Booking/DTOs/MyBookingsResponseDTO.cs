using Application.Common.Patterns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.DTOs
{
    public class MyBookingsResponseDTO
    {
        public long BookingId { get; set; }

        public string Category { get; set; }

        public string Title { get; set; }     

        public decimal TotalPrice { get; set; }

        public string Currency { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
