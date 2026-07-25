using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ICalculateNightPrice
    {
        Task<decimal> TotalBookingPrice(decimal price_per_night,
                                                           DateOnly check_in_date,
                                                           DateOnly check_out_date,
                                                           CancellationToken cancellationToken);
    }
}
