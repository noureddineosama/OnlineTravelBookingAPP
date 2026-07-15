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

        public SearchRoomHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<PaginatedResult<GetHotelRoomsResponseDTO>> Handle(SearchRoomQuery request, CancellationToken cancellationToken)
        {
            var room_isntance = unitOfWork.Repository<room>();
            if (room_isntance == null)
                throw new ArgumentNullException(nameof(room_isntance));

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
            return paginated_result;
        }
    }
}
