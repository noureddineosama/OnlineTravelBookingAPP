using Application.Common.Patterns;
using Application.Features.HotelBooking.DTOs;
using Application.Features.HotelBooking.Handlers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Commands
{
    public sealed record CreateHotelBookingCommand(CreateHotelBookingRequestDTO requestDTO)    
                                                                : IRequest<GenericResult<CreateHotelBookingResponseDTO>>;
}
