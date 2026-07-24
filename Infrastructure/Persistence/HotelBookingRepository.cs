using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence
{
    public class HotelBookingRepository : IHotelBookingRepository
    {
        private readonly AppDbContext dbContext;

        public HotelBookingRepository(AppDbContext dbContext )
        {
            this.dbContext = dbContext;
        }

        public async Task<int> ExecuteUpdateAsync(long roomId, DateOnly check_in_date , DateOnly check_out_date, 
                                                CancellationToken cancellationToken)
        {
            var execute_update_result = await dbContext.room_availabilities.
                               Where(op => op.room_id == roomId &&
                                           op.date >= check_in_date
                                           && op.date < check_out_date)
                               .ExecuteUpdateAsync(op => op.SetProperty(op => op.IsAvailable, true));
            return execute_update_result;
        }
    }
}
