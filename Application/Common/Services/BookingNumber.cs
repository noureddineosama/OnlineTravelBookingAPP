using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Services
{
    public static class BookingNumber
    {
        public static string GeneratBookingNumber()
        {
            return "CAR-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
    }
}
