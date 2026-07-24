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

        public async Task<bool> ValidateDatesAsync(DateOnly check_in_date,
                                                   DateOnly check_out_date,
                                                   List<room_availability> room_Availabilities,
                                                   CancellationToken cancellationToken)
        {
            //. Validating if the new checking in less than the existing check out 

            //. if not exist that's meaning that this date is available 
            //List<room_availability> checking_availabilities = room_Availabilities.Where(aval => aval.date >= check_in_date &&
            //                                                                                      aval.date < check_out_date && 
            //                                                                                      aval.IsAvailable == true)
            //                                                                                      .ToList();

            //if (checking_availabilities.Any())
            //return true;


            //. very law performance i will take a step in doing this better
            foreach (var aval in room_Availabilities)
            {
                if (check_in_date <= aval.date &&
                    check_out_date > aval.date && 
                    !aval.IsAvailable) //. if there is one day that the is available is not true return false
                { 
                        return false;
                }
            }
            return true;
        }
    }
}
    