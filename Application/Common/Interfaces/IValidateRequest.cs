using Application.Features.Images.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IValidateRequest
    {
        void ValidateRequestUploadImage(UploadImageRequestDTO request, CancellationToken cancellationToken);
        void ValidateRequests(IReadOnlyCollection<UploadImageRequestDTO> requests, CancellationToken cancellationToken);
    }
}
