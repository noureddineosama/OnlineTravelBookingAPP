using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelBooking.Commands;
using Application.Features.HotelBooking.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
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

        public UpdateHotelBookingHandler(IUnitOfWork unitOfWork,  
                                         ICalculateNumberOfNights  nights,
                                         ICheckAvailabilityRoom checkAvailability,
                                         IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.nights = nights;
            this.checkAvailability = checkAvailability;
            this.mapper = mapper;
        }

        async Task<GenericResult<UpdateHotelBookingResponseDTO>> IRequestHandler<UpdateHotelBookingCommand, GenericResult<UpdateHotelBookingResponseDTO>>.Handle(UpdateHotelBookingCommand request, CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid Occurred");

            //. Getting hotel booking with tracking process
            var existing_hotel_booking = await hotel_booking_instance.GetByIdAsync(op => op.id == request.id );
            if (existing_hotel_booking == null)
                return await Result.FailureAsync<UpdateHotelBookingResponseDTO>("Booking not found ");

            //. updating data in memory (or Mapping )
            var hotel_booking_mapped = mapper.Map(request.requestDTO, existing_hotel_booking);

            //. Mapping data to another DTO 
            var checkRoomAvailability = mapper.Map<CheckRoomAvailabilityRequestDTO>(hotel_booking_mapped);

            if (await checkAvailability.ValidateDatesAsync(checkRoomAvailability, hotel_booking_mapped, cancellationToken) == false)
                return await Result.FailureAsync<UpdateHotelBookingResponseDTO>("This booking can't be updated. ");

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
                    price_override = hotel_booking_mapped.room.price_per_night,
                    room_id = hotel_booking_mapped.room_id
                });
            }
            //. adding new records to the availabilities date for the room
            await room_availability_instance.AddBulkDataAsync(room_availabilities,cancellationToken);


            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<UpdateHotelBookingResponseDTO>(new UpdateHotelBookingResponseDTO
            {
                Adults = hotel_booking_mapped.guests_adults,
                Children = hotel_booking_mapped.guests_children,
                CheckInDate = hotel_booking_mapped.check_in_date,
                CheckOutDate = hotel_booking_mapped.check_out_date,
                BookingStatus = hotel_booking_mapped.booking.status.ToString(),
                HotelBookingId = hotel_booking_mapped.id,
                NumberOfNights = nights.NumberOfNights(hotel_booking_mapped, cancellationToken),
                PricePerNight = hotel_booking_mapped.price_per_night,
                Quantity = hotel_booking_mapped.quantity,
                SubTotal = hotel_booking_mapped.booking.subtotal,
                Message = "Booking had been updated successfully"
            });
        }
    }
}