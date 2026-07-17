using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelBooking.DTOs;
using Application.Features.HotelBooking.Queries;
using Domain.Entities;
using MediatR;
using System;
using Domain.Enums;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Application.Features.HotelBooking.Handlers
{
    public sealed class CancelHotelBookingHandler : IRequestHandler<CancelHotelBookingQuery, GenericResult<CancelHotelBookingResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;

        public CancelHotelBookingHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<GenericResult<CancelHotelBookingResponseDTO>> Handle(CancelHotelBookingQuery request, CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid occurred !!");

            var existing_hotel_booking = await hotel_booking_instance.GetByIdAsync(op => op.id == request.id);
            if (existing_hotel_booking == null)
                return await Result.FailureAsync<CancelHotelBookingResponseDTO>("Booking not found.");

            existing_hotel_booking.booking.status = BookingStatus.Cancelled.ToString();
            existing_hotel_booking.booking.IsCancelled = true;

            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<CancelHotelBookingResponseDTO>(new CancelHotelBookingResponseDTO
            {
                Success = true,
                Message = "Booking had been cancelled"
            });
        }
    }
}
