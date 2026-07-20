using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Images.DTOs
{
    public class  UploadImageRequestDTO
    {
        public required Stream FileStream { get; init; }

        public required string FileName { get; init; }

        public required string ContentType { get; init; }

        public required string FolderName { get; init; }
    }
}
