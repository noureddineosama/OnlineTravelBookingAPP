using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces;
using Application.Features.Images.DTOs;
using Infrastructure.AWSSettings;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
            foreach(var image in requests)
            {
                responses.Add(await UploadImageAsync(image, cancellationToken));
            }

            return responses;
        }

        //public async Task<UploadImageResponseDTO> DeleteImageAsync(string ObjectKey,
        //                                                            CancellationToken cancellationToken)
        //{

        //}
    }
}
