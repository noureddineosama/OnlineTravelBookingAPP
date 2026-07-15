using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Hotels.Commands;
using Application.Features.Hotels.DTOs;
using AutoMapper;
using Domain.Entities;
using FluentValidation.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class UpdateHotelHandler : IRequestHandler<UpdateHotelCommand, GenericResult<UpdateHotelResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IGenerateSlug generateSlug;
        private readonly IMapper mapper;
        private readonly ICurrentIUserService currentIUser;

        public UpdateHotelHandler(IUnitOfWork unitOfWork,
                                  IGenerateSlug generateSlug, 
                                  IMapper mapper, 
                                  ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.generateSlug = generateSlug;
            this.mapper = mapper;
            this.currentIUser = currentIUser;
        }

        public async Task<GenericResult<UpdateHotelResponseDTO>> Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
        {
            var hotel_instance = unitOfWork.Repository<hotel>();
            if (hotel_instance == null)
                throw new ArgumentNullException(nameof(hotel_instance));

            var hotel = await hotel_instance.GetByIdAsync(op => op.id == request.id);
            if (hotel == null)
                return await Result.FailureAsync<UpdateHotelResponseDTO>("Hotel not found. ");

            if (!await hotel_instance.AnyAsync(op => op.location_id == request.requestDTO.location_id, cancellationToken))
                return await Result.FailureAsync<UpdateHotelResponseDTO>("Location not found. ");

            var updated_hotel_mapped = mapper.Map(request.requestDTO, hotel);
            updated_hotel_mapped.status = request.requestDTO.Status.ToString();
            updated_hotel_mapped.updated_at = DateTime.UtcNow;
            updated_hotel_mapped.UpdatedBy = currentIUser.UserId;

            string slug = generateSlug.generateSlug(updated_hotel_mapped, cancellationToken);

            //. check duplicating for slug values 
            if (await hotel_instance
                .AnyAsync(op => op.slug == slug, cancellationToken))
                return await Result.FailureAsync<UpdateHotelResponseDTO>("This slug is already exist. ");
            updated_hotel_mapped.slug = slug;

            await unitOfWork.SaveChangesAsync();

            var response = mapper.Map<UpdateHotelResponseDTO>(updated_hotel_mapped);

            return await Result.SuccessAsync<UpdateHotelResponseDTO>(response);
        }
    }
}
