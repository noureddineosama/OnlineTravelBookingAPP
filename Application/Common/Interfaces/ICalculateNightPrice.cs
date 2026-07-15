using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ICalculateNightPrice
    {
        Task<decimal> TotalBookingPrice(hotel_booking booking,
                                                           CancellationToken cancellationToken);
    }
}
