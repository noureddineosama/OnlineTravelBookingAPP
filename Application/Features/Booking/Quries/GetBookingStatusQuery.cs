using Application.Common.Patterns;
using Application.Features.Booking.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.Quries
{
    public sealed record GetBookingStatusQuery(long id) : IRequest<GenericResult<GetBookingStatusResponseDTO>>;
}
