using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelAvailability.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelAvailability.Handlers
{
    public sealed class CheckRoomAvailabilityHandler : IRequestHandler<CheckRoomQuery, GenericResult<CheckRoomAvailabilityResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICheckAvailabilityRoom checkAvailability;
        private readonly Common.Interfaces.ICachService<hotel_booking> cachService;

        public CheckRoomAvailabilityHandler(IUnitOfWork unitOfWork, 
                                            ICheckAvailabilityRoom checkAvailability,
                                            ICachService<hotel_booking> cachService )
        {
            this.unitOfWork = unitOfWork;
            this.checkAvailability = checkAvailability;
            this.cachService = cachService;
        }

        //. this service or mothod for if the user asking for a specific room and is the room is available in that time or not
        public async Task<GenericResult<CheckRoomAvailabilityResponseDTO>> Handle(CheckRoomQuery request, CancellationToken cancellationToken)
        {
            var instance = unitOfWork.Repository<hotel_booking>();
            if(instance == null)
                throw new ArgumentNullException(nameof(instance));
            var room_instance = unitOfWork.Repository<room>();
            if (room_instance == null)
                throw new ArgumentNullException(nameof(instance));

            var existing_room = await room_instance.GetByIdAsync(op => op.id == request.requestDTO.room_id &&
                                                                        op.status == "Active" &&
                                                                        op.IsDeleted == false, cancellationToken);
            if(existing_room is null ) return await Result.FailureAsync<CheckRoomAvailabilityResponseDTO>("Room not found. ");
            if (existing_room.room_availabilities.Any()) {
                if (await checkAvailability.ValidateDatesAsync(request.requestDTO.check_in_date, request.requestDTO.check_out_date,existing_room.room_availabilities.ToList(), cancellationToken) == false)
                    return await Result.FailureAsync<CheckRoomAvailabilityResponseDTO>("Room is not available now.");
            }

            return await Result.SuccessAsync<CheckRoomAvailabilityResponseDTO>(new CheckRoomAvailabilityResponseDTO
            {
                IsAvailable = true,
                Message = "Room is Available",
            });
        }
    }
}
