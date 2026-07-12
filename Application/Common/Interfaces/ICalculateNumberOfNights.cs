using Application.Features.HotelBooking.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ICalculateNumberOfNights
    {
        int NumberOfNights(hotel_booking requestDTO,
                                       CancellationToken cancellationToken);
    }
}
