using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelBooking.DTOs;
using Application.Features.HotelBooking.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Handlers
{
    public sealed class HotelBookingDetailsHandler : IRequestHandler<HotelBookingDetailsQuery, GenericResult<HotelBookingDetailsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;

        public HotelBookingDetailsHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<GenericResult<HotelBookingDetailsResponseDTO>> Handle(HotelBookingDetailsQuery request, CancellationToken cancellationToken)
        {
            var instace_Of_hotel_booking = unitOfWork.Repository<hotel_booking>();
            if (instace_Of_hotel_booking == null)
                throw new ArgumentNullException("Something invalid occurred!!");
            var existing_booking = await instace_Of_hotel_booking.GetByIdAsync(predicate: op => op.id == request.id);
            if (existing_booking == null)
                return await Result.FailureAsync<HotelBookingDetailsResponseDTO>("Booking not found.");

            return await Result.SuccessAsync<HotelBookingDetailsResponseDTO>(new HotelBookingDetailsResponseDTO
            {
                Adults = existing_booking.guests_adults,
                CheckInDate = existing_booking.check_in_date,
                CheckOutDate = existing_booking.check_out_date,
                BookingId = existing_booking.booking_id,
                Children = existing_booking.guests_children,
                HotelName = existing_booking.room.hotel.name,
                PricePerNight = existing_booking.price_per_night,
                RoomName = existing_booking.room.name,
                Status = existing_booking.booking.status.ToString(),
                TotalPrice = existing_booking.booking.total_price
            });
        }
    }
}
