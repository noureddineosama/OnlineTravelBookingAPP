using Application.Common.Interfaces;
using Application.Common.Patterns;
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
        private readonly IMapper mapper;

        public UpdateHotelBookingHandler(IUnitOfWork unitOfWork,  
                                         ICalculateNumberOfNights  nights,
                                         IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.nights = nights;
            this.mapper = mapper;
        }

        async Task<GenericResult<UpdateHotelBookingResponseDTO>> IRequestHandler<UpdateHotelBookingCommand, GenericResult<UpdateHotelBookingResponseDTO>>.Handle(UpdateHotelBookingCommand request, CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid Occurred");

            var existing_hotel_booking = await hotel_booking_instance.GetByIdAsync(op => op.id == request.id );
            if (existing_hotel_booking == null)
                return await Result.FailureAsync<UpdateHotelBookingResponseDTO>("Booking not found ");

            var hotel_booking_mapped = mapper.Map(request.requestDTO, existing_hotel_booking);
            await unitOfWork.SaveChangesAsync();

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