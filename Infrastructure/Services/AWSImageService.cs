using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces;
using Application.Features.Images.DTOs;
using Infrastructure.AWSSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Infrastructure.Services
{
    public class AWSImageService : IAWSImageService
    {
        private readonly IAmazonS3 amazonS3;
        private readonly IOptions<AwsSettings> options;
        private readonly IValidateRequest validateRequest;

        public AWSImageService(IAmazonS3 amazonS3,
                               IOptions<AwsSettings> options,
                               IValidateRequest validateRequest)
        {
            this.amazonS3 = amazonS3;
            this.options = options;
            this.validateRequest = validateRequest;
        }
        public async Task<UploadImageResponseDTO> UploadImageAsync(UploadImageRequestDTO request,
                                                              CancellationToken cancellationToken = default)
        {
            validateRequest.ValidateRequestUploadImage(request, cancellationToken);

            var extension = Path.GetExtension(request.FileName);
            var generatedFileName =
                $"{Guid.NewGuid()}{extension}";

            var objectKey =
                $"{request.FolderName.Trim('/')}/{generatedFileName}"; //. --> folderName / {Guid}.extension

            var putObjectRequest = new PutObjectRequest
            {
                BucketName = options.Value.BucketName,

                Key = objectKey,

                InputStream = request.FileStream,

                ContentType = request.ContentType,

                AutoCloseStream = false
            };

            //. Sending file to AWS 

            var response =
                await amazonS3.PutObjectAsync(
                    putObjectRequest,
                    cancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception("Uploading image to AWS S3 failed.");

            var imageUrl =
                $"https://{options.Value.BucketName}.s3.{options.Value.Region}.amazonaws.com/{objectKey}";

            return new UploadImageResponseDTO
            {
                ImageUrl = imageUrl, //. that will be uploaded in db 
                
                ObjectKey = objectKey,

                FileName = generatedFileName 
            };
        }

        public async Task<IReadOnlyCollection<UploadImageResponseDTO>> UploadImagesAsync(IReadOnlyCollection<UploadImageRequestDTO> requests, 
                                                                                         CancellationToken cancellationToken)
        {
            validateRequest.ValidateRequests(requests, cancellationToken);

            foreach (var request in requests)
                validateRequest.ValidateRequestUploadImage(request, cancellationToken);

            List<UploadImageResponseDTO> responses = [];
            //. the current practice that i did 
            foreach(var image in requests)
            {
                responses.Add(await UploadImageAsync(image, cancellationToken));
            }

            return responses;
        }

        public async Task DeleteImageAsync(string ObjectKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(ObjectKey))
                throw new ArgumentException("object key is required", nameof(ObjectKey));


            var DeleteRequest = new DeleteObjectRequest
            {
                BucketName = options.Value.BucketName,
                Key = ObjectKey
            };

            var response = await amazonS3.DeleteObjectAsync(DeleteRequest, cancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK ||
                response.HttpStatusCode != HttpStatusCode.NoContent)
                throw new InvalidOperationException("Failed to delete image from AWS. ");
        }

        public async Task DeleteImagesAsync(IReadOnlyCollection<string> ObjectKeys
                                          , CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ObjectKeys);

            if (!ObjectKeys.Any())
                throw new ArgumentNullException(nameof(ObjectKeys));

            foreach( var item in ObjectKeys)
            {
                await DeleteImageAsync(item, cancellationToken);
            }
        }

        public async Task<UploadImageResponseDTO> ReplaceImageAsync(
                                            string OldobjectKey,
                                            UploadImageRequestDTO requestDTO,
                                            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(OldobjectKey))
                throw new ArgumentException("Old object key is required", nameof(OldobjectKey));

            await DeleteImageAsync(OldobjectKey,
                                   cancellationToken);

            return await UploadImageAsync(requestDTO , cancellationToken);
        }

        public async Task<IReadOnlyList<UploadImageResponseDTO>> ReplaceImagesAsync(string ObjectKey
                                                                                    ,IReadOnlyCollection<UploadImageRequestDTO> requestDtOs, 
                                                                                    CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(requestDtOs);

            validateRequest.ValidateRequests(requestDtOs, cancellationToken);

            List<UploadImageResponseDTO> responseDTOs = [];
            foreach(var item in requestDtOs)
            {
                responseDTOs.Add(await ReplaceImageAsync(ObjectKey,
                                              item, cancellationToken));
            }

            return responseDTOs;
        }
    }
}
