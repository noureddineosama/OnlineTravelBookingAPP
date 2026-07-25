using Application.Common.Services;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IBookingService
    {
       Task<booking> CreateBookingAsync(
                                  long userId,
                                  string category,
                                  decimal subtotal,
                                  decimal totalPrice,
                                  string currency,
                                  CancellationToken cancellationToken);

       Task UdpateBookingStatusAsync(long bookingId,
                                     string newBookingStatus,
                                     CancellationToken cancelltionToken);

       Task UpdateBookingPriceAsync(long bookingId,
                                    decimal subTotal,
                                    decimal TotalPrice,
                                    CancellationToken cancellationToken);

       Task CancelBookingStatusAsync(long bookingId,
                                                   CancellationReasonType reasonType,
                                                   string? details,
                                                   CancellationToken cancellationToken);
    }
}
