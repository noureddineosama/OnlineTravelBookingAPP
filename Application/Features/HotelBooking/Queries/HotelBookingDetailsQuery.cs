using Application.Common.Patterns;
using Application.Features.HotelBooking.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Queries
{
    public sealed record HotelBookingDetailsQuery(long id) : IRequest<GenericResult<HotelBookingDetailsResponseDTO>>;
}
