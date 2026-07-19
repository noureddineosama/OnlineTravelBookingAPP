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
            if (requestDTO.check_in_date > booking.check_in_date  && requestDTO.check_in_date != requestDTO.check_out_date)
            {
                if (requestDTO.check_in_date < booking.check_out_date)
                    return false;
            }
            return true; 
        }
    }
}
    