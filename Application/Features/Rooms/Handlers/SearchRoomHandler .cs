using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Hotels.DTOs;
using Application.Features.Rooms.DTOs;
using Application.Features.Rooms.Queries;
using AutoMapper.Configuration.Annotations;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public class SearchRoomHandler : IRequestHandler<SearchRoomQuery, PaginatedResult<GetHotelRoomsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<PaginatedResult<GetHotelRoomsResponseDTO>> cachService;

        public SearchRoomHandler(IUnitOfWork unitOfWork,
                                 ICachService<PaginatedResult<GetHotelRoomsResponseDTO>> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
        }
        public async Task<PaginatedResult<GetHotelRoomsResponseDTO>> Handle(SearchRoomQuery request, CancellationToken cancellationToken)
        {
            var room_isntance = unitOfWork.Repository<room>();
            if (room_isntance == null)
                throw new ArgumentNullException(nameof(room_isntance));

            var cach_result = await cachService.GetAsync($"get room for hotel id : {request.hotelId}" +
                                                         $"{request.page}-" +
                                                         $"{request.pageSize}-" +
                                                         $"{request.requestDTO.MaxPrice}-" +
                                                         $"{request.requestDTO.MinPrice}-" +
                                                         $"{request.requestDTO.BedType}", cancellationToken);
            if (cach_result is not null)
                return cach_result;

            var paginated_result = await room_isntance.GetPaginationAsync(predicate: op => op.hotel_id == request.hotelId && 
                                                                                         op.IsDeleted == false
                                                                                         && op.status == "Active",
                                                                          selector: op => new GetHotelRoomsResponseDTO
                                                                          {
                                                                              BedType = op.bed_type,
                                                                              CoverImage = op.room_images.Where(opt => opt.id == op.id).
                                                                                            Select(op => op.url).FirstOrDefault(),
                                                                              Name = op.name,
                                                                              PricePerNight = op.price_per_night,
                                                                              Refundable = op.refundable,
                                                                              RoomId= op.id
                                                                          }, page:request.page, pageSize: request.pageSize, cancellationToken:cancellationToken
                                                                          );
            await cachService.SetAsync($"get room for hotel id : {request.hotelId}" +
                                                         $"{request.page}-" +
                                                         $"{request.pageSize}-" +
                                                         $"{request.requestDTO.MaxPrice}-" +
                                                         $"{request.requestDTO.MinPrice}-" +
                                                         $"{request.requestDTO.BedType}", paginated_result, cancellationToken);
            return paginated_result;
        }
    }
}
