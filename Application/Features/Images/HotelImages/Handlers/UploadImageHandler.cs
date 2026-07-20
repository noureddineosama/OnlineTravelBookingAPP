using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Images.DTOs;
using Application.Features.Images.HotelImages.Commands;
using Domain.Entities;
using MediatR;
using Stripe;
using Stripe.Treasury;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Images.HotelImages.Handlers
{
    public sealed class UploadImageHandler : IRequestHandler<CreateHotelImageCommand, GenericResult<IReadOnlyCollection<UploadImageResponseDTO>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IAWSImageService aWSImage;

        public UploadImageHandler(IUnitOfWork unitOfWork , IAWSImageService aWSImage)
        {
            this.unitOfWork = unitOfWork;
            this.aWSImage = aWSImage;
        }

        public async Task<GenericResult<IReadOnlyCollection<UploadImageResponseDTO>>> Handle(CreateHotelImageCommand request, CancellationToken cancellationToken)
        {
            var hotel_instance = unitOfWork.Repository<hotel>();
            if(hotel_instance is null)throw new ArgumentNullException(nameof(hotel_instance));

            var existing_hotel = await hotel_instance.GetByIdAsync(predicate: op => op.id == request.hotel_id &&
                                                                               op.IsDeleted == false, cancellationToken);
            if (existing_hotel is null) return await Result.FailureAsync<IReadOnlyCollection<UploadImageResponseDTO>>("Hotel not found. ");

            var hotel_image_instance = unitOfWork.Repository<hotel_image>();
            if (hotel_image_instance is null) throw new ArgumentNullException(nameof(hotel_image_instance));

            IReadOnlyCollection<UploadImageResponseDTO> result = await aWSImage.UploadImagesAsync(request.requestDTO, cancellationToken);

            foreach(var item in result)
            {
                //.low performance for creating an instance (in RAM) for each iteration with bringing the id from hotel table
                await hotel_image_instance.AddAsync(new hotel_image
                {
                    hotel_id = existing_hotel.id,
                    url = item.ImageUrl
                           ,
                    sort_order = default
                }, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<IReadOnlyCollection<UploadImageResponseDTO>>(result);
            
        }
    }
}
