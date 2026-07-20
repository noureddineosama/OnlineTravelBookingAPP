using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Images.DTOs
{
    public class UploadImageResponseDTO
    {
        public required string ImageUrl { get; init; }

        public required string ObjectKey { get; init; }

        public required string FileName { get; init; }
    }
}
