using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.DTOs
{
    public class CancelBookingRequestDTO
    {
        public string CancellationReason { get; set; }
        public CancellationReasonType cancellationReasonType { get; set; }

        public string? Notes { get; set; }
    }
}
