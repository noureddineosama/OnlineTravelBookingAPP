using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Rooms.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public sealed class UpdateRoomPriceHandler : IRequestHandler<UpdateRoomPriceCommand, GenericResult<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentUser;

        public UpdateRoomPriceHandler(IUnitOfWork unitOfWork,
                                      ICurrentIUserService currentUser)
        {
            this.unitOfWork = unitOfWork;
            this.currentUser = currentUser;
        }
        public async Task<GenericResult<string>> Handle(UpdateRoomPriceCommand request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            var existing_room = await room_instance.GetByIdAsync(predicate: op => op.id == request.roomId && op.IsDeleted == false
                                                                                && op.status == "Active",
                                                                                cancellationToken: cancellationToken);
            if (existing_room == null)
                return await Result.FailureAsync<string>("Room not found. ");

            existing_room.UpdatedBy = currentUser.UserId;
            existing_room.updated_at = DateTime.UtcNow;
            existing_room.price_per_night = request.requestDTO.PricePerNight;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<string>("Price changed successfully. ");
        }
    }
}
