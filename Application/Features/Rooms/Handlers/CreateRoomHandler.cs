using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Rooms.Commands;
using Application.Features.Rooms.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, GenericResult<CreateRoomResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ICurrentIUserService currentIUser;

        public CreateRoomHandler(IUnitOfWork unitOfWork, 
                                 IMapper mapper,
                                 ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<CreateRoomResponseDTO>> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            var hotel_instance = unitOfWork.Repository<hotel>();
            if(hotel_instance == null)
                throw new ArgumentNullException(nameof(hotel_instance));

            if (!await hotel_instance.AnyAsync(op => op.id == request.hotelId, cancellationToken))
                return await Result.FailureAsync<CreateRoomResponseDTO>("Hotel not found. ");

            var room_instance = unitOfWork.Repository<room>();
            if(room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            var room_mapped = mapper.Map<room>(request.requestDTO);
            room_mapped.hotel_id = request.hotelId;
            room_mapped.created_at = DateTime.UtcNow;
            room_mapped.CreatedBy = currentIUser.UserId;

            await room_instance.AddAsync(room_mapped, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<CreateRoomResponseDTO>(new CreateRoomResponseDTO
            {
                Id = room_mapped.id,
                Name = room_mapped.name,
                PricePerNight = room_mapped.price_per_night,
                Status = room_mapped.status
            }, "Room created successfully");
        }
    }
}
