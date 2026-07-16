using Application.Common.Patterns;
using Application.Features.Hotels.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Commands
{
    public sealed record DeleteHotelCommand(long id) : IRequest<GenericResult<DeleteHotelResponseDTO>>;
}
 