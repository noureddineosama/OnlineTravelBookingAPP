using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelBooking.DTOs;
using Application.Features.HotelBooking.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Handlers
{
    public sealed class HotelBookingDetailsHandler : IRequestHandler<HotelBookingDetailsQuery, GenericResult<HotelBookingDetailsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<hotel_booking> cacheService;
        private readonly ICalculateNightPrice calculateNightPrice;

        public HotelBookingDetailsHandler(IUnitOfWork unitOfWork,
                                          ICachService<hotel_booking> cacheService,
                                          ICalculateNightPrice calculateNightPrice)
        {
            this.unitOfWork = unitOfWork;
            this.cacheService = cacheService;
            this.calculateNightPrice = calculateNightPrice;
        }
        public async Task<GenericResult<HotelBookingDetailsResponseDTO>> Handle(HotelBookingDetailsQuery request, CancellationToken cancellationToken)
        {
            var instace_Of_hotel_booking = unitOfWork.Repository<hotel_booking>();
            if (instace_Of_hotel_booking == null)
                throw new ArgumentNullException("Something invalid occurred!!");

            await cacheService.GetAsync("existing-booking", cancellationToken);

            var existing_booking = await instace_Of_hotel_booking.GetByIdAsync(predicate: op => op.id == request.id,
                                                                               cancellationToken, op => op.room, book => book.booking, hote => hote.room.hotel);
            if (existing_booking == null)
                return await Result.FailureAsync<HotelBookingDetailsResponseDTO>("Booking not found.");

            await cacheService.SetAsync("existing-booking", existing_booking, cancellationToken);

            //. 3 includes must be done in this response

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
                TotalPrice = existing_booking.TotalPrice
            }, "Data recieved successfully. ");
        }
    }
}
