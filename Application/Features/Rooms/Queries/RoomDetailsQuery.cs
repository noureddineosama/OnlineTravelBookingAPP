using Application.Common.Patterns;
using Application.Features.Rooms.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Queries
{
    public sealed record RoomDetailsQuery(long id) : IRequest<GenericResult<RoomDetailsResponseDTO>>;
}
