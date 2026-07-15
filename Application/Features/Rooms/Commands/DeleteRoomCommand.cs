using Application.Common.Patterns;
using Application.Features.Rooms.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Commands
{
    public sealed record DeleteRoomCommand(long id) : IRequest<GenericResult<DeleteRoomResponseDTO>>;
}
