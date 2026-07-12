using Application.Common.Patterns;
using Application.Features.HotelBooking.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Commands
{
    public sealed record UpdateHotelBookingCommand(long id, UpdateHotelBookingRequestDTO requestDTO) : IRequest<GenericResult<UpdateHotelBookingResponseDTO>>;
}
