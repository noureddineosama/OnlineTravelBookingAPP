using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelBooking.Commands;
using Application.Features.HotelBooking.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CreateHotelBookingHandler> logger;
        private readonly IMapper mapper;

        public CreateHotelBookingHandler(IUnitOfWork unitOfWork,  
                                         ICheckAvailabilityRoom checkAvailability,
                                         ICalculateNightPrice calculateNightPrice,
                                         ILogger<CreateHotelBookingHandler> logger,
                                         IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.checkAvailability = checkAvailability;
            this.calculateNightPrice = calculateNightPrice;
            this.logger = logger;
            this.mapper = mapper;
        }
        public async Task<GenericResult<CreateHotelBookingResponseDTO>> Handle(CreateHotelBookingCommand request,
                                                                               CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid occurred ");

            var existing_booing_Using_Room_Id = await hotel_booking_instance.GetByIdAsync(op => op.room_id == request.requestDTO.rooom_id);
            if (existing_booing_Using_Room_Id == null)
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room not found");

            var data_mapped = mapper.Map<CheckRoomAvailabilityRequestDTO>(request.requestDTO);

            if (!await checkAvailability.ValidateDatesAsync(data_mapped, existing_booing_Using_Room_Id, cancellationToken))
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room can't be booked");

            var room = unitOfWork.Repository<room>();
            if (room == null)
                throw new ArgumentNullException("Something invalid occurred");

            //.Getting Room using the id that will be inputed from the user request
            var bringing_room_using_room_id = await room.GetByIdAsync(predicate: op => op.id == request.requestDTO.rooom_id);
            if (bringing_room_using_room_id == null)
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room not found");

            if (bringing_room_using_room_id.status != "active")
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room is not active now.");

            if (request.requestDTO.quantity > await checkAvailability.CalculateRemainingRooms(existing_booing_Using_Room_Id, cancellationToken))
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("This quantity is not available right now.");

            //. Creating new hotel booking
            var new_hotel_booking = mapper.Map<hotel_booking>(request.requestDTO);

            //. The Price per night of hotel is the same price for the price per night for the room
            new_hotel_booking.price_per_night = bringing_room_using_room_id.price_per_night;

            //. calculate total price 

            var calculate_total_price = await calculateNightPrice.TotalBookingPrice(new_hotel_booking, cancellationToken);
            if (calculate_total_price == 0)
            {
                logger.LogWarning("Take a look in your financial class `calculate Night Price or Total Price`");
                throw new Exception("Something went wrong !!");
            }
            //.Create Booking 


            //. Create Hotel Booking 
            await hotel_booking_instance.AddAsync(new_hotel_booking);
            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<CreateHotelBookingResponseDTO>(new CreateHotelBookingResponseDTO
            {
                BookingId = new_hotel_booking.booking_id,
                CheckInDate = new_hotel_booking.check_in_time,
                CheckOutDate = new_hotel_booking.check_out_time,
                HotelBookingId= new_hotel_booking.id,
                HotelName = new_hotel_booking.room.hotel.name,
                Status= new_hotel_booking.room.hotel.status,
                PricePerNight= new_hotel_booking.price_per_night,
                RoomName = new_hotel_booking.room.name,
                TotalPrice = new_hotel_booking.booking.total_price
            });
        }
    }
}
