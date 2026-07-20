using Application.Common.Patterns;
using Application.Features.Images.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Images.HotelImages.Commands
{
    public sealed record CreateHotelImageCommand(long hotel_id, IReadOnlyCollection<UploadImageRequestDTO> requestDTO)
                                                    : IRequest<GenericResult<IReadOnlyCollection<UploadImageResponseDTO>>>;
}
