using Application.Common.Patterns;
using Application.Features.CarBookings.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.CarBookings.Queries
{
    public sealed record MyCarBookingsQuery : IRequest<GenericResult<List<MyCarBookingsResponseDTO>>>;
}
