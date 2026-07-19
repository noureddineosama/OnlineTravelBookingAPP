using Application.Common.Patterns;
using Application.Features.CarBookings.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CarBookings.Commands
{
    public sealed record CreateCarBookingCommand(CreateCarBookingRequestDTO requestDTO)
                                                    : IRequest<GenericResult<CarBookingResponse>>;
}
