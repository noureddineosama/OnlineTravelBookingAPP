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

        public CheckRoomAvailabilityHandler(IUnitOfWork unitOfWork, 
                                            ICheckAvailabilityRoom checkAvailability)
        {
            this.unitOfWork = unitOfWork;
            this.checkAvailability = checkAvailability;
        }

        //. this service or mothod for if the user asking for a specific room and is the room is available in that time or not
        public async Task<GenericResult<CheckRoomAvailabilityResponseDTO>> Handle(CheckRoomQuery request, CancellationToken cancellationToken)
        {
            var instance = unitOfWork.Repository<hotel_booking>();
            if(instance == null)
                throw new ArgumentNullException(nameof(instance));

            var result = await instance.GetByIdAsync(predicate: op => op.room_id == request.requestDTO.RoomId, cancellationToken);
            if(result == null)
                return await Result.FailureAsync<CheckRoomAvailabilityResponseDTO>(nameof(result));

            if (!await checkAvailability.ValidateDatesAsync(request.requestDTO, result, cancellationToken))
                return await Result.FailureAsync<CheckRoomAvailabilityResponseDTO>("Room is not available now.");

            if (await checkAvailability.CalculateRemainingRooms(result, cancellationToken) == 0)
                return await Result.FailureAsync<CheckRoomAvailabilityResponseDTO>("You can not book this booking");

            return await Result.SuccessAsync<CheckRoomAvailabilityResponseDTO>(new CheckRoomAvailabilityResponseDTO
            {
                IsAvailable = true,
                Message = "Room is Available",
                //.here you need to know the number of the rooms that will be available in the time that the customer enetered
                RemainingRooms = await checkAvailability.CalculateRemainingRooms(result, cancellationToken)
            });
        }
    }
}
