using Application.Common.Patterns;
using Application.Features.Rooms.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Commands
{
    public sealed record CreateRoomCommand(long hotelId,CreateRoomRequestDTO requestDTO) : IRequest<GenericResult<CreateRoomResponseDTO>>;
}
