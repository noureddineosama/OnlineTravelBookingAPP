using Application.Common.Patterns;
using Application.Features.Rooms.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Application.Features.Rooms.Commands
{
    public sealed record UpdateRoomExtraCommand(long roomId,UpdateRoomExtraRequestDTO requestDTO) : IRequest<GenericResult<UpdateRoomExtraResponseDTO>>;
}
