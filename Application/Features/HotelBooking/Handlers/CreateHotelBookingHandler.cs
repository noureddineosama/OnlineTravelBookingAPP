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
        private readonly ICachService<hotel_booking> cachService;

        public CreateHotelBookingHandler(IUnitOfWork unitOfWork,  
                                         ICheckAvailabilityRoom checkAvailability,
                                         ICalculateNightPrice calculateNightPrice,
                                         ILogger<CreateHotelBookingHandler> logger,
                                         IMapper mapper, 
                                         ICachService<hotel_booking> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.checkAvailability = checkAvailability;
            this.calculateNightPrice = calculateNightPrice;
            this.logger = logger;
            this.mapper = mapper;
            this.cachService = cachService;
        }

        //. first: Validate the booking for the same check in and out date in room available table 
        //. after that if didn't exist, that's meaning that i will validate the conflicts for the booking 
        //. if exist i will reject this booking 
        //. if not exist that's meaning that i will create a new room_aval record from check_in time and check_out and create this booking 
        //. but create parent booking and after that create the hotel_booking 
        //. first problem we will use the SaveChangesAsync two times --> when we create a new room
        public async Task<GenericResult<CreateHotelBookingResponseDTO>> Handle(CreateHotelBookingCommand request,
                                                                               CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid occurred ");

            await cachService.GetAsync("existing-hotel-booking", cancellationToken);

            //. first validating if there is booking for this data  
            var existing_booking_Using_Room_Id = await hotel_booking_instance.GetByIdAsync(op => op.room_id == request.requestDTO.room_id && 
                                                                                                 op.check_out_date > op.check_in_date &&
                                                                                                 op.check_in_date == request.requestDTO.check_in_date&& 
                                                                                                 op.check_out_date == request.requestDTO.check_out_date && 
                                                                                                 op.room.status == "Active");
            if (existing_booking_Using_Room_Id != null)
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room is not Available");


            var check_room_aval_request = mapper.Map<CheckRoomAvailabilityRequestDTO>(request.requestDTO);

            //. and after that validate if there are conflicts existing with the booking 
            if (await hotel_booking_instance.AnyAsync(op => op.room_id == request.requestDTO.room_id &&
                                                      op.check_out_date > op.check_in_date &&
                                                      op.check_in_date < request.requestDTO.check_in_date
                                                      , cancellationToken))
            {
                if (await checkAvailability.ValidateDatesAsync(check_room_aval_request, existing_booking_Using_Room_Id, cancellationToken))
                    return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room can't be booked");
            }

            await cachService.SetAsync("existing-hotel-booking", existing_booking_Using_Room_Id, cancellationToken);

            var room_available_instance = unitOfWork.Repository<room_availability>();
            if(room_available_instance == null)
                throw new ArgumentNullException(nameof(room_available_instance));

            //. room created here due to if there are no bookings in the table will give me an exception of null reference 
            var room = unitOfWork.Repository<room>();
            if (room_available_instance == null)
                throw new ArgumentNullException(nameof(room_available_instance));

            var existing_room = await room.GetByIdAsync(op => op.id == request.requestDTO.room_id 
                                                            && op.status == "Active"
                                                            ,cancellationToken);
            if (existing_room == null)
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room not found. ");

            //.Create room available records 
            var room_availability_instance = unitOfWork.Repository<room_availability>();
            if (room_availability_instance is null) throw new ArgumentNullException(nameof(room_availability_instance));

            var room_availabilities = new List<room_availability>();

            //. from check in date to before last day of the check out due to we don't booking the leaving day 
            for (var day = request.requestDTO.check_in_date; day < request.requestDTO.check_out_date; day = day.AddDays(1))
            {
                room_availabilities.Add(new room_availability
                {
                    date = day,
                    IsAvailable = false, //. due to this booking will be booked now 
                    price_override = existing_room.price_per_night,
                    room_id = existing_room.id
                });
            }

            await room_availability_instance.AddBulkDataAsync(room_availabilities, cancellationToken);

            //. Creating new hotel booking 

            var new_hotel_booking = mapper.Map<hotel_booking>(request.requestDTO);

            //. The Price per night of hotel is the same price for the price per night for the room
            new_hotel_booking.price_per_night = existing_room.price_per_night;

            //. calculate total price 

            var calculate_total_price = await calculateNightPrice.TotalBookingPrice(existing_room.price_per_night,
                                                       request.requestDTO.check_in_date, request.requestDTO.check_out_date,
                                                       request.requestDTO.quantity, cancellationToken);
            if (calculate_total_price == 0)
            {
                logger.LogWarning("Take a look in your financial class `calculate Night Price or Total Price`");
                throw new Exception("Something went wrong !!");
            }
            //.Create Booking 

            //. there is booking will be created and after that we will add in it the totalPrice and subprice and display it on the customer

            //. Create Hotel Booking 
            await hotel_booking_instance.AddAsync(new_hotel_booking);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<CreateHotelBookingResponseDTO>(new CreateHotelBookingResponseDTO
            {
                BookingId = new_hotel_booking.booking_id,
                CheckInDate = new_hotel_booking.check_in_date,
                CheckOutDate = new_hotel_booking.check_out_date,
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
