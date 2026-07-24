using Application.Common.Patterns;
using Application.Features.Booking.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.Commands
{
    public sealed record CancelBookingCommand(long bookingId,CancelBookingRequestDTO requestDTO) : IRequest<GenericResult<CancelBookingResponseDTO>>;
}
