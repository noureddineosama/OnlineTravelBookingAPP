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
    public class GetRoomExtrasHandler : IRequestHandler<GetRoomExtrasQuery, GenericResult<RoomExtrasResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<RoomExtrasResponseDTO> cachService;

        public GetRoomExtrasHandler(IUnitOfWork unitOfWork, 
                                    ICachService<RoomExtrasResponseDTO> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
        }
        public async Task<GenericResult<RoomExtrasResponseDTO>> Handle(GetRoomExtrasQuery request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if (room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            var cach_result = await cachService.GetAsync($"get-room-extras with room {request.roomId}", cancellationToken);
            
            if (!await room_instance.AnyAsync(predicate: op => op.id == request.roomId &&
                                                                op.IsDeleted == false &&
                                                                op.status == "Active",
                                                                cancellationToken))
                return await Result.FailureAsync<RoomExtrasResponseDTO>("Room not found. ");

            if (cach_result is not null)
                return await Result.SuccessAsync<RoomExtrasResponseDTO>(cach_result);

            RoomExtrasResponseDTO room_extras = await room_instance.GetSelectorAsync(
                                                                             predicate: op => op.id == request.roomId,
                                                                             selector: op => new RoomExtrasResponseDTO
                                                                             {
                                                                                 roomId = op.id,
                                                                                 extras = op.room_extras.Select(op => new UpdateRoomExtraResponseDTO
                                                                                 {
                                                                                     Id = op.id,
                                                                                     Name= op.name,
                                                                                     Price =op.price
                                                                                 }).ToList()
                                                                             });

            await cachService.SetAsync($"get-room-extras with room {request.roomId}",room_extras, cancellationToken);

            return await Result.SuccessAsync<RoomExtrasResponseDTO>(room_extras);
        }
    }
}
