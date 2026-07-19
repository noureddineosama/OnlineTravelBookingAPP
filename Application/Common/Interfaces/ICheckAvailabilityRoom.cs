using Application.Features.HotelAvailability.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ICheckAvailabilityRoom
    {
        Task<bool> ValidateDatesAsync(CheckRoomAvailabilityRequestDTO requestDTO, hotel_booking booking,
                                                  CancellationToken cancellationToken);
    }
}
