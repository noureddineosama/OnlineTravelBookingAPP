using Application.Common.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class CalculateNightPrice : ICalculateNightPrice
    {
        public async Task<decimal> TotalBookingPrice(hotel_booking booking,
                                                           CancellationToken cancellationToken)  
        { 
            int nights = (booking.check_out_time.DayNumber - booking.check_in_time.DayNumber);
            if (nights == 0)
                throw new InvalidOperationException("Invalid booking dates.");

            //. quantity --> requested_rooms  
            decimal totalPrice = (booking.quantity * booking.price_per_night * nights);

            totalPrice -= booking.booking.discount_amount;

            return totalPrice;
        }
    }
}
