using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Rooms.DTOs;
using Application.Features.Rooms.Queries;
using Domain.Entities;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public class HotelRoomsHandler : IRequestHandler<GetHotelRoomsQuery, PaginatedResult<GetHotelRoomsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<PaginatedResult<GetHotelRoomsResponseDTO>> cachService;

        public HotelRoomsHandler(IUnitOfWork unitOfWork, 
                                ICachService<PaginatedResult<GetHotelRoomsResponseDTO>> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
        }
        public async Task<PaginatedResult<GetHotelRoomsResponseDTO>> Handle(GetHotelRoomsQuery request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null) 
                throw new ArgumentNullException(nameof(room_instance));

            var cach_result = await cachService.GetAsync($"get-hotels-rooms{request.page}-" +
                                                         $"{request.pageSize}-" +
                                                         $"{request.hotelId}-", cancellationToken);
            if (cach_result != null)
                return cach_result;

            var result = await room_instance.GetPaginationAsync(predicate: op => op.hotel_id == request.hotelId
                                                                                 && op.IsDeleted == false,
                                                                selector: op => new GetHotelRoomsResponseDTO
                                                                {
                                                                    BedType = op.bed_type,
                                                                    CoverImage = op.room_images.Where(opt => opt.room_id == op.id) 
                                                                                            .Select(op => op.url).FirstOrDefault(),//. the first image url for the rooms
                                                                    Name= op.name,
                                                                    PricePerNight = op.price_per_night,
                                                                    Refundable = op.refundable,
                                                                    RoomId =  op.id
                                                                },
                                                                 page: request.page,
                                                                 pageSize: request.pageSize,
                                                                 cancellationToken: cancellationToken,
                                                                 includes: op => op.room_images);

            await cachService.SetAsync($"get-hotels-rooms{request.page}-" +
                                                         $"{request.pageSize}-" +
                                                         $"{request.hotelId}-", result, cancellationToken);
            
            return result;
        }
    }
}
