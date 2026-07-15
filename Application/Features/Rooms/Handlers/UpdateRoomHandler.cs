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
    public sealed class UpdateRoomHandler : IRequestHandler<UpdateRoomCommand, GenericResult<UpdateRoomResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public UpdateRoomHandler(IUnitOfWork  unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<GenericResult<UpdateRoomResponseDTO>> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null) 
                throw new ArgumentNullException(nameof(room_instance));

            var room = await room_instance.GetByIdAsync(op => op.id == request.roomId, cancellationToken);
            if (room == null)
                return await Result.FailureAsync<UpdateRoomResponseDTO>("Room not found. ");

            var updated_room = mapper.Map<room>(request.requestDTO);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<UpdateRoomResponseDTO>(new UpdateRoomResponseDTO
            {
                Updated = true
            });
        }
    }
}
