using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelBooking.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Application.Services
{
    public class CheckAvailabilityRoom : ICheckAvailabilityRoom
    {
        private readonly ILogger<CheckAvailabilityRoom> logger;

        public CheckAvailabilityRoom(ILogger<CheckAvailabilityRoom> logger)
        {
            this.logger = logger;
        }

        public async Task<bool> ValidateDatesAsync(CheckRoomAvailabilityRequestDTO requestDTO,
                                                   hotel_booking booking,
                                                   CancellationToken cancellationToken)
        {
            //. Validating if the new checking in less than the existing check out 
            if (requestDTO.CheckInDate > booking.check_in_date  && requestDTO.CheckInDate != requestDTO.CheckOutDate)
            {
                if (requestDTO.CheckInDate < booking.check_out_date)
                    return false;
            }
            return true; 
        }

        //public async Task<> CheckAvailability(CreateHotelBookingRequestDTO requestdTO, hotel_booking booking)
        //{
        //    var list_Invalid_Bookings = await booking.
        //}


        //. Validate the quantity with the value that will be returned 
        public async Task<int> CalculateRemainingRooms(hotel_booking booking, CancellationToken cancellationToken)
        {
            //. Min(2 in date .... ,3 in date ... ,4 in date ... ,12 in date ... ,8 in date ...)
            //                --> taking least number  of rooms due to this is the least value for the available rooms 
            int remainingRooms = booking.room.room_availabilities.Min(op => op.available_units);
            if (remainingRooms == 0)
            {
                logger.LogWarning("There are no rooms Aval !!!!!!!!!");
                return 0;
            }
            return remainingRooms; //. returning the minimum number of rooms that will be available at this time
        }
    }
}
    