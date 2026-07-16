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
    public class CreateRoomExtraHandler : IRequestHandler<CreateRoomExtraCommand, GenericResult<CreateRoomExtraResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CreateRoomExtraHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<GenericResult<CreateRoomExtraResponseDTO>> Handle(CreateRoomExtraCommand request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));
            if (request.roomId <= 0)
                return await Result.FailureAsync<CreateRoomExtraResponseDTO>("Validation Failed. ");

            if (!await room_instance.AnyAsync(predicate: op => op.id == request.roomId && op.IsDeleted == false &&
                                                                 op.status == "Active", cancellationToken))
                return await Result.FailureAsync<CreateRoomExtraResponseDTO>("Room not found. ");

            var room_extra_instance = unitOfWork.Repository<room_extra>();
            if (room_extra_instance == null)
                throw new ArgumentNullException(nameof(room_instance));
            room_extra room_extra_mapped = mapper.Map<room_extra>(request.requestDTO);
            room_extra_mapped.room_id = request.roomId;

            await room_extra_instance.AddAsync(room_extra_mapped);
            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<CreateRoomExtraResponseDTO>(new CreateRoomExtraResponseDTO
            {
                Id = room_extra_mapped.id,
                Name = room_extra_mapped.name,
                Price = room_extra_mapped.price
            });
        }
    }
}
