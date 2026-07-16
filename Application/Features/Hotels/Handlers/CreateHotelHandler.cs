using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Hotels.Commands;
using Application.Features.Hotels.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class CreateHotelHandler : IRequestHandler<CreateHotelCommand, GenericResult<CreateHotelResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IGenerateSlug generateSlug;

        public CreateHotelHandler(IUnitOfWork unitOfWork,
                                  IGenerateSlug generateSlug)
        {
            this.unitOfWork = unitOfWork;
            this.generateSlug = generateSlug;
        }
        public async Task<GenericResult<CreateHotelResponseDTO>> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentException(nameof(request));

            var hotel_instance = unitOfWork.Repository<hotel>();
            if (hotel_instance == null)
                throw new ArgumentNullException(nameof(hotel_instance));

            var location_instance = unitOfWork.Repository<location>();
            if (location_instance == null)
                throw new ArgumentNullException(nameof(location_instance));

            await location_instance.AddAsync(new location
            {
                address_line = request.requestDTO.location.address_line,
                city = request.requestDTO.location.city,
                country = request.requestDTO.location.country,
                latitude = request.requestDTO.location.latitude,
                longitude = request.requestDTO.location.longitude,
            });
            var hotel = new hotel
            {
                check_in_time = request.requestDTO.CheckInTime,
                check_out_time = request.requestDTO.CheckOutTime,
                description = request.requestDTO.Description,
                created_at = DateTime.UtcNow,
                name = request.requestDTO.Name,
                star_rating = request.requestDTO.StarRating,
                main_image_url = default,
                status = request.requestDTO.Status.ToString(),
            };
            hotel.slug = generateSlug.generateSlug(hotel);
            

            if (!await hotel_instance.AnyAsync(op => op.slug == hotel.slug, cancellationToken))
                return await Result.FailureAsync<CreateHotelResponseDTO>("Slug is already exist. ");

            await hotel_instance.AddAsync(hotel);
            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<CreateHotelResponseDTO>(new CreateHotelResponseDTO
            {
                HotelId = hotel.id,
                Name = hotel.name,
                Slug = hotel.slug,
                CreatedAt = hotel.created_at,
                Status = hotel.status
            });
        }
    }
}
