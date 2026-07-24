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
using Stripe;
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
        private readonly ICurrentIUserService currentIUserService;
        private readonly IBookingService bookingService;
        private readonly ICacheInvalidationService cacheInvalidationService;

        public CreateHotelBookingHandler(IUnitOfWork unitOfWork,  
                                         ICheckAvailabilityRoom checkAvailability,
                                         ICalculateNightPrice calculateNightPrice,
                                         ILogger<CreateHotelBookingHandler> logger,
                                         IMapper mapper,
                                         ICurrentIUserService currentIUserService,
                                         IBookingService bookingService,
                                         ICacheInvalidationService cacheInvalidationService
                                          )
        {
            this.unitOfWork = unitOfWork;
            this.checkAvailability = checkAvailability;
            this.calculateNightPrice = calculateNightPrice;
            this.logger = logger;
            this.mapper = mapper;
            this.currentIUserService = currentIUserService;
            this.bookingService = bookingService;
            this.cacheInvalidationService = cacheInvalidationService;
        }
        public async Task<GenericResult<CreateHotelBookingResponseDTO>> Handle(CreateHotelBookingCommand request,
                                                                               CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance is null) throw new ArgumentNullException(nameof(hotel_booking_instance));

            var room_instance = unitOfWork.Repository<room>();
            if (room_instance is null) throw new ArgumentNullException(nameof(room_instance));

            //. Validate existing room 
            var existing_room = await room_instance.GetByIdAsync(predicate: op => op.id == request.requestDTO.room_id &&
                                                                        op.IsDeleted == false &&
                                                                        op.status == "Active",
                                                                        cancellationToken,
                                                                        op => op.room_availabilities,
                                                                        op => op.hotel
                                                                        );

            if (existing_room is null) return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room not found. ");

            //. validating for existing room availabilities values or records recorded for the existing room
            var existing_room_availabilities_for_specfic_room_id = existing_room.room_availabilities
                                                                    .Where(op => op.room_id == request.requestDTO.room_id &&
                                                                                  op.IsAvailable == false
                                                                                  ).ToList();
            if (existing_room_availabilities_for_specfic_room_id.Any() == true ) 
            {
                    if (await checkAvailability
                       .ValidateDatesAsync(request.requestDTO.check_in_date, request.requestDTO.check_out_date, 
                                                                                existing_room_availabilities_for_specfic_room_id.Where(op =>
                                                                                                  op.date >= request.requestDTO.check_in_date &&
                                                                                                  op.date < request.requestDTO.check_out_date).ToList(),
                                                                                cancellationToken) == false)
                    return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Room is not available at this date. ");
            }
                         
            var room_availability_instance = unitOfWork.Repository<room_availability>();
            if (room_availability_instance is null) throw new ArgumentNullException(nameof(room_availability_instance));


            //.Create room available records 
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

            decimal totalprice = await calculateNightPrice.TotalBookingPrice(existing_room.price_per_night,
                                                                        request.requestDTO.check_in_date,
                                                                        request.requestDTO.check_out_date,
                                                                        cancellationToken);
            if (totalprice == 0)
                return await Result.FailureAsync<CreateHotelBookingResponseDTO>("Something invalid occurred. ");

            //. Creating Parent  Booking 
           booking booking = await bookingService.CreateBookingAsync(currentIUserService.UserId,
                                                    "hotel",
                                                    totalprice,
                                                    totalprice,
                                                    "EGP",
                                                    cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            //.Caching that responsible for bringing booking's user must be deleted after each creation or updating process

            cacheInvalidationService.Invalidate(currentIUserService.UserId, cancellationToken);

            //. Manual mapping for the hotel booking entity
            var hotel_Booking = new hotel_booking
            {
                check_in_date = request.requestDTO.check_in_date,
                check_out_date = request.requestDTO.check_out_date,
                guests_children = request.requestDTO.guests_children,
                price_per_night = existing_room.price_per_night,
                quantity = 1,
                room_id = existing_room.id,
                guests_adults = request.requestDTO.guests_adults,
                TotalPrice = totalprice,
                booking_id = booking.id
            };

            await hotel_booking_instance.AddAsync(hotel_Booking, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateHotelBookingResponseDTO
            {
                BookingId = booking.id,
                CheckInDate = request.requestDTO.check_in_date,
                CheckOutDate = request.requestDTO.check_out_date,
                HotelBookingId = hotel_Booking.id,
                HotelName = existing_room.hotel.name,
                PricePerNight = existing_room.price_per_night,
                RoomName = existing_room.name,
                Status = booking.status,
                TotalPrice = totalprice
            };
            return await Result.SuccessAsync<CreateHotelBookingResponseDTO>(response, "Data recieved successfully. ");
        }
    }
}
