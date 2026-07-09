using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Application.Services
{
    public class CheckAvailabilityRoom : ICheckAvailabilityRoom
    {

        public async Task<bool> ValidateDatesAsync(CheckRoomAvailabilityRequestDTO requestDTO, hotel_booking booking,
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



        public async Task<int> CalculateRemainingRooms(hotel_booking booking, CancellationToken cancellationToken)
        {
            //. This depending on the calculating of the AvailableUnits that existing in the available rooms model
            int remainingRooms = booking.room.room_availabilities.Select(op => op.available_units).Count();
            if (remainingRooms == 0)
                return 0;  
            return remainingRooms; //. you can book this booking 
        }
    }
}
    