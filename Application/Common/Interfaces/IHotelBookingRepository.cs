using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IHotelBookingRepository
    {
        Task<int> ExecuteUpdateAsync(long roomId, DateOnly check_in_date, DateOnly check_out_date,
                                                CancellationToken cancellationToken);
    }
}
