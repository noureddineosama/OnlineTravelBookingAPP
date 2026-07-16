using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Hotels.Commands;
using Application.Features.Rooms.Commands;
using Application.Features.Rooms.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public sealed class ChangeRoomStatusHandler : IRequestHandler<ChangeStatusCommand,GenericResult<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;

        public ChangeRoomStatusHandler(IUnitOfWork unitOfWork, ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<string>> Handle(ChangeStatusCommand request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null) 
                throw new ArgumentNullException(nameof(room_instance));

            if (request.roomId <= 0)
                throw new ArgumentException("Validation failed. ");
             
            var existing_room =  await room_instance.GetByIdAsync(predicate: op => op.id ==  request.roomId && op.IsDeleted == false);
            if (existing_room == null)
                return await Result.FailureAsync<string>("Room not found. ");
            existing_room.UpdatedBy = currentIUser.UserId;
            existing_room.updated_at = DateTime.UtcNow;
            existing_room.status = request.requestDTO.Status.ToString();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<string>("Status Changed Successfully. ");
        }
    }
}
