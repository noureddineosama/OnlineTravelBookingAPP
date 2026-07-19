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
    public class UpdateAvailabilityHandler : IRequestHandler<UpdateAvailabilityCommand, GenericResult<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;

        public UpdateAvailabilityHandler(IUnitOfWork unitOfWork
                                       , ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<string>> Handle(UpdateAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room_availability>();
            if (room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            var existing_room = await room_instance.GetByIdAsync(op => op.room_id == request.roomId);
            if (existing_room == null)
                return await Result.FailureAsync<string>("Room not found. ");

            existing_room.IsAvailable = request.requestDTO.IsAvailable;
            existing_room.price_override = request.requestDTO.PriceOverride;
            existing_room.date = request.requestDTO.Date;
            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<string>("updated Successfully. ");
        }
    }
}
