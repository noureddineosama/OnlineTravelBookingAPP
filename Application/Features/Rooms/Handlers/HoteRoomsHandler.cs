using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Rooms.DTOs;
using Application.Features.Rooms.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public class HoteRoomsHandler : IRequestHandler<GetHotelRoomsQuery, PaginatedResult<GetHotelRoomsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;

        public HoteRoomsHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<PaginatedResult<GetHotelRoomsResponseDTO>> Handle(GetHotelRoomsQuery request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null) 
                throw new ArgumentNullException(nameof(room_instance));

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
            return result;
        }
    }
}
