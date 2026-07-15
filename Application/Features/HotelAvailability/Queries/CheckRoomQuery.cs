using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelAvailability.Queries
{
    public sealed record CheckRoomQuery(CheckRoomAvailabilityRequestDTO requestDTO) 
                                                    : IRequest<GenericResult<CheckRoomAvailabilityResponseDTO>>;
}