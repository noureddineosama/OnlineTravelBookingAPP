using Application.Features.Images.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IAWSImageService
    {
        Task<UploadImageResponseDTO> UploadImageAsync(UploadImageRequestDTO request,
                                                              CancellationToken cancellationToken = default);
    }
}
