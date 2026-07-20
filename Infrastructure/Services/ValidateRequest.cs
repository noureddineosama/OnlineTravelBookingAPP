using Amazon.Runtime.Internal.Util;
using Application.Common.Interfaces;
using Application.Features.Images.DTOs;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class ValidateRequest : IValidateRequest
    {
        private static List<string> allowedExtensions = new List<string> { ".png", ".jpg", ".jpeg", ".webp" };//. Validate extensions
        private const long MaxImageSize = 5 * 1024 * 1024; //. Maximum size   
        public void ValidateRequestUploadImage(UploadImageRequestDTO request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.FileStream is null)
                throw new ArgumentNullException(nameof(request.FileStream));

            if (request.FileStream.Length == 0)
                throw new Exception("Image stream is empty.");

            if (string.IsNullOrWhiteSpace(request.FileName))
                throw new Exception("File name is required.");

            if (string.IsNullOrWhiteSpace(request.ContentType))
                throw new Exception("Content type is required.");

            if (string.IsNullOrWhiteSpace(request.FolderName))
                throw new Exception("Folder name is required.");

            var extension = Path.GetExtension(request.FileName);

            if (string.IsNullOrWhiteSpace(extension))
                throw new Exception("Invalid file extension.");
            if (!allowedExtensions.Contains(extension))
                throw new Exception($"Extension {extension} isn't allowed.");

            if (request.FileStream.Length > MaxImageSize)
                throw new InvalidOperationException(
                    "Image exceeded max size.");
        }

        public void ValidateRequests(IReadOnlyCollection<UploadImageRequestDTO> requests, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(requests);

            if (!requests.Any())
                throw new ArgumentNullException(nameof(requests));
            if (requests.Count > 10)
                throw new InvalidOperationException("Maximum allowed images is 10 ");
        }
    }
}
