using Application.Common.Patterns;
using Application.Features.Hotels.DTOs;
using Application.Features.Rooms.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Queries
{
    public sealed record SearchRoomQuery(long hotelId,int page,int pageSize,SearchRoomRequestDTO requestDTO) : IRequest<PaginatedResult<GetHotelRoomsResponseDTO>>;
}
