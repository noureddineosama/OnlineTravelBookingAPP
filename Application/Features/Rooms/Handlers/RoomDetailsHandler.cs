using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Rooms.DTOs;
using Application.Features.Rooms.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public sealed class RoomDetailsHandler : IRequestHandler<RoomDetailsQuery, GenericResult<RoomDetailsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<RoomDetailsResponseDTO> cachService;

        public RoomDetailsHandler(IUnitOfWork unitOfWork, 
                                  ICachService<RoomDetailsResponseDTO> cachService)  
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
        }
        public async Task<GenericResult<RoomDetailsResponseDTO>> Handle(RoomDetailsQuery request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            var cach_result = await cachService.GetAsync($"room-details with id : {request.id}", 
                                                            cancellationToken);
            if (cach_result is not null)
                return await Result.SuccessAsync<RoomDetailsResponseDTO>(cach_result);
            
            var existing_room = await room_instance.GetSelectorAsync(
                               predicate: op => op.id == request.id && op.IsDeleted == false && 
                                             op.status == "Active",
                               selector: op => new RoomDetailsResponseDTO
                               {
                                   Adults = op.occupancy_adults,
                                   BedType = op.bed_type,
                                   Children = op.occupancy_children,
                                   Name = op.name,
                                   Id = op.id,
                                   PricePerNight = op.price_per_night,
                                   Refundable = op.refundable,
                                   Status = op.status,
                                   Images = op.room_images.Select(op => new RoomImageDTO
                                   {
                                       Id=  op.id,
                                       Url = op.url
                                   }).ToList(),
                                   Extras = op.room_extras.Select(op => new CreateRoomExtraResponseDTO
                                   {
                                       Id = op.id,
                                       Name = op.name,
                                       Price = op.price
                                   }).ToList()
                               });

            if (existing_room == null)
                return await Result.FailureAsync<RoomDetailsResponseDTO>("Room not found. ");

            await cachService.SetAsync($"room-details with id : {request.id}", existing_room, cancellationToken);

            return await Result.SuccessAsync<RoomDetailsResponseDTO>(existing_room);
        }
    }
}
