using Application.Common.Patterns;
using Application.Features.Rooms.DTOs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Queries
{
    public sealed record GetHotelRoomsQuery(long hotelId, int page, int pageSize) : IRequest<PaginatedResult<GetHotelRoomsResponseDTO>>;
}
