using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Rooms.Commands;
using Application.Features.Rooms.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public class DeleteRoomHandler : IRequestHandler<DeleteRoomCommand, GenericResult<DeleteRoomResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;

        public DeleteRoomHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<GenericResult<DeleteRoomResponseDTO>> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            //. this getting by id is a tracking process (don't forget)
            var existing_room = await room_instance.GetByIdAsync(op => op.id == request.id && op.IsDeleted == false);
            if (existing_room == null)
                return await Result.FailureAsync<DeleteRoomResponseDTO>("Room not found. ");
            existing_room.IsDeleted = true;
            existing_room.DeletedAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<DeleteRoomResponseDTO>(new DeleteRoomResponseDTO
            {
                Deleted = true
            });
        }
    }
}
