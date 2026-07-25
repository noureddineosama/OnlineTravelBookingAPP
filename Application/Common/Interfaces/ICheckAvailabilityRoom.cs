using Application.Features.HotelAvailability.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ICheckAvailabilityRoom
    {
        Task<bool> ValidateDatesAsync(DateOnly check_in_date,
                                                   DateOnly check_out_date,
                                                   List<room_availability> room_Availabilities,
                                                   CancellationToken cancellationToken);
    }
}
