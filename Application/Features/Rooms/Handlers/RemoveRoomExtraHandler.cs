using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Rooms.Commands;
using Application.Features.Rooms.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public sealed class RemoveRoomExtraHandler : IRequestHandler<RemoveRoomExtraCommand, GenericResult<string>>
    {
        private readonly IUnitOfWork unitOfWork;

        public RemoveRoomExtraHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<GenericResult<string>> Handle(RemoveRoomExtraCommand request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if (room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));
            if (request.roomId <= 0)
                return await Result.FailureAsync<string>("Validation Failed. ");

            if (!await room_instance.AnyAsync(predicate: op => op.id == request.roomId && op.IsDeleted == false &&
                                                                 op.status == "Active", cancellationToken))
                return await Result.FailureAsync<string>("Room not found. ");

            IRepository<room_extra> room_extra_instance = unitOfWork.Repository<room_extra>();
            if (room_extra_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            //. getting tracking process for the tracking processes 
            var room_extra = await room_extra_instance.GetByIdAsync(predicate: op => op.id == request.id
                                                                                  && op.room_id == request.roomId, cancellationToken);

            if (room_extra == null)
                return await Result.FailureAsync<string>("Room extra not found. ");

            room_extra_instance.Remove(room_extra);
            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<string>("Extra removed successfully. ");
        }
    }
}
