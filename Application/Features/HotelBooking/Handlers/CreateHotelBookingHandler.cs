using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelBooking.Commands;
using Application.Features.HotelBooking.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.HotelBooking.Handlers
{
    public sealed class CreateHotelBookingHandler : IRequestHandler<CreateHotelBookingCommand, GenericResult<CreateHotelBookingResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICheckAvailabilityRoom checkAvailability;
        private readonly ICalculateNightPrice calculateNightPrice;
        private readonly IMapper mapper;

        public CreateHotelBookingHandler(IUnitOfWork unitOfWork,  
                                         ICheckAvailabilityRoom checkAvailability,
                                         ICalculateNightPrice calculateNightPrice,
                                         IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.checkAvailability = checkAvailability;
            this.calculateNightPrice = calculateNightPrice;
            this.mapper = mapper;
        }
        public async Task<GenericResult<CreateHotelBookingResponseDTO>> Handle(CreateHotelBookingCommand request, 
                                                                               CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid occurred ");

            var existing_booing_Using_Room_Id = await hotel_booking_instance.GetByIdAsync(op => op.room_id == request.requestDTO.RoomId);
            if (existing_booing_Using_Room_Id == null)
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room not found");

            var data_mapped = mapper.Map<CheckRoomAvailabilityRequestDTO>(request.requestDTO);

            if (!await checkAvailability.ValidateDatesAsync(data_mapped, existing_booing_Using_Room_Id, cancellationToken))
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room can't be booked");

            var room = unitOfWork.Repository<room>();
            if (room == null)
                throw new ArgumentNullException("Something invalid occurred");

            var bringing_room_using_room_id = await room.GetByIdAsync(predicate: op => op.id == request.requestDTO.RoomId);
            if (bringing_room_using_room_id == null)
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room not found");


        }
    }
}
