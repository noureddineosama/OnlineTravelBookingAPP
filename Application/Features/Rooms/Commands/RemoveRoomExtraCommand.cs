using Application.Common.Patterns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Commands
{
    public sealed record RemoveRoomExtraCommand(long roomId, long id) : IRequest<GenericResult<string>>;
}
