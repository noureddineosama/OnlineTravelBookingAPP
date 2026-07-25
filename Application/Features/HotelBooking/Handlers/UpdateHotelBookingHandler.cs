using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelBooking.Commands;
using Application.Features.HotelBooking.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Stripe.V2.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Handlers
{
    public sealed class UpdateHotelBookingHandler : IRequestHandler<UpdateHotelBookingCommand, GenericResult<UpdateHotelBookingResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICalculateNumberOfNights nights;
        private readonly ICheckAvailabilityRoom checkAvailability;
        private readonly IMapper mapper;
        private readonly ICalculateNightPrice calPrice;
        private readonly ICurrentIUserService currentIUserService;
        private readonly ICacheInvalidationService cacheInvalidationService;

        public UpdateHotelBookingHandler(IUnitOfWork unitOfWork,  
                                         ICalculateNumberOfNights  nights,
                                         ICheckAvailabilityRoom checkAvailability,
                                         IMapper mapper,  
                                         ICalculateNightPrice calPrice,
                                         ICurrentIUserService currentIUserService,
                                         ICacheInvalidationService cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.nights = nights;
            this.checkAvailability = checkAvailability;
            this.mapper = mapper;
            this.calPrice = calPrice;
            this.currentIUserService = currentIUserService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        async Task<GenericResult<UpdateHotelBookingResponseDTO>> IRequestHandler<UpdateHotelBookingCommand, GenericResult<UpdateHotelBookingResponseDTO>>.Handle(UpdateHotelBookingCommand request, CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid Occurred");

            //. Getting hotel booking with tracking process
            var existing_hotel_booking = await hotel_booking_instance.GetByIdAsync(op => op.id == request.id && 
                                                                                        op.booking.status == "Pending" && 
                                                                                        op.booking.IsCancelled == false && 
                                                                                        op.booking.IsDeleted == false ,
                                                                                        cancellationToken ,
                                                                                        op => op.booking,
                                                                                        op => op.room,
                                                                                        op => op.room.room_availabilities
                                                                                    );
            if (existing_hotel_booking == null)
                return await Result.FailureAsync<UpdateHotelBookingResponseDTO>("Booking not found ");

            if (await checkAvailability.ValidateDatesAsync(request.requestDTO.check_in_date , request.requestDTO.check_out_date, existing_hotel_booking
                                                                                                                                .room.room_availabilities
                                                                                                                                .Where(op => op.date >= request.requestDTO.check_in_date && 
                                                                                                                                             op.date < request.requestDTO.check_out_date).ToList(), cancellationToken) == false)
                return await Result.FailureAsync<UpdateHotelBookingResponseDTO>("This booking can't be updated, This booking is not available");

            //. Calcuating total Price for updating for updating Hotel Booking 

            var hotel_Booking_Total_Price = await calPrice.TotalBookingPrice(existing_hotel_booking.room.price_per_night,
                                                request.requestDTO.check_in_date,
                                                request.requestDTO.check_out_date,cancellationToken);
            if (hotel_Booking_Total_Price == 0)
                return await Result.FailureAsync<UpdateHotelBookingResponseDTO>("Something invalid occurred. ");


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
                    price_override = existing_hotel_booking.room.price_per_night,
                    room_id = existing_hotel_booking.room_id
                });
            }
            //. adding new records to the availabilities date for the room
            await room_availability_instance.AddBulkDataAsync(room_availabilities,cancellationToken);

            existing_hotel_booking.guests_adults = request.requestDTO.guests_adults;
            existing_hotel_booking.guests_children = request.requestDTO.guests_children;
            existing_hotel_booking.check_in_date = request.requestDTO.check_in_date;
            existing_hotel_booking.check_out_date = request.requestDTO.check_out_date;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            cacheInvalidationService.Invalidate(currentIUserService.UserId, cancellationToken);

            return await Result.SuccessAsync<UpdateHotelBookingResponseDTO>(new UpdateHotelBookingResponseDTO
            {
                Adults = existing_hotel_booking.guests_adults,
                Children = existing_hotel_booking.guests_children,
                CheckInDate = existing_hotel_booking.check_in_date,
                CheckOutDate = existing_hotel_booking.check_out_date,
                BookingStatus = existing_hotel_booking.booking.status.ToString(),
                HotelBookingId = existing_hotel_booking.id,
                NumberOfNights = nights.NumberOfNights(existing_hotel_booking, cancellationToken),
                PricePerNight = existing_hotel_booking.room.price_per_night,
                SubTotal = hotel_Booking_Total_Price,
                Message = "Booking had been updated successfully"
            });
        }
    }
}