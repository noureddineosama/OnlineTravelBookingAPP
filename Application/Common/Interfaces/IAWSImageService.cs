using Application.Features.Images.DTOs;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IAWSImageService
    {
        Task<UploadImageResponseDTO> UploadImageAsync(UploadImageRequestDTO request,
                                                 CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<UploadImageResponseDTO>> UploadImagesAsync(IReadOnlyCollection<UploadImageRequestDTO> requests,
                                                                            CancellationToken cancellationToken);
       
        Task DeleteImageAsync(string ObjectKey, CancellationToken cancellationToken);
        Task DeleteImagesAsync(IReadOnlyCollection<string> ObjectKeys
                             , CancellationToken cancellationToken);
       
        Task<UploadImageResponseDTO> ReplaceImageAsync(
                              string OldobjectKey,
                              UploadImageRequestDTO requestDTO,
                              CancellationToken cancellationToken);
        Task<IReadOnlyList<UploadImageResponseDTO>> ReplaceImagesAsync(string ObjectKey
                                                                                    , IReadOnlyCollection<UploadImageRequestDTO> requestDtOs,
                                                                                    CancellationToken cancellationToken);
    }
}
