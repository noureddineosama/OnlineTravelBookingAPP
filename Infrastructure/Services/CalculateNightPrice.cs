using Application.Common.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class CalculateNightPrice : ICalculateNightPrice
    {
        //. Price per night will be for the room 
        public async Task<decimal> TotalBookingPrice(decimal price_per_night,
                                                           DateOnly check_in_date,
                                                           DateOnly check_out_date,
                                                           CancellationToken cancellationToken)  
        { 
            int nights = (check_out_date.DayNumber -check_in_date.DayNumber);
            if (nights == 0)
                throw new InvalidOperationException("Invalid booking dates.");

            //. quantity --> requested_rooms  
            decimal totalPrice = (price_per_night * nights);

            return totalPrice;
        }
    }
}
