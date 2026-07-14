using Application.Common.Interfaces;
using Application.Features.HotelBooking.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class CalculateNumberOfNights : ICalculateNumberOfNights 
    {
        public int NumberOfNights(hotel_booking requestDTO,
                                       CancellationToken cancellationToken)
        {
            var number_Of_nights = requestDTO.check_out_date.DayNumber - requestDTO.check_out_date.DayNumber;
            return number_Of_nights;
        }
    }
}
