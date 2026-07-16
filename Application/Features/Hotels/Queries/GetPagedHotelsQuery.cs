using Application.Common.Patterns;
using Application.Features.Hotels.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Queries
{
    public sealed record GetPagedHotelsQuery(GetHotelsRequestDTO requestDTO) : IRequest<PaginatedResult<GetHotelsResponseDTO>>;
}
