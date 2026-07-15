using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CarBookings.DTOs
{
    public sealed class CarExtraResponse
    {
        public string Name { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal Price { get; init; }
    }
}
