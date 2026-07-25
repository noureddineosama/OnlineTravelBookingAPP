using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.HotelBooking.DTOs;
using Application.Features.HotelBooking.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Handlers
{
    public sealed class MyHotelBookingsHandler : IRequestHandler<MyHotelBookingQuery, GenericResult<List<MyHotelBookingsResponseDTO>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;
        private readonly ICachService<List<MyHotelBookingsResponseDTO>> cachService;

        public MyHotelBookingsHandler(IUnitOfWork unitOfWork,
                                ICurrentIUserService currentIUser, 
                                ICachService<List<MyHotelBookingsResponseDTO>> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
            this.cachService = cachService;
        }
        public async Task<GenericResult<List<MyHotelBookingsResponseDTO>>> Handle(MyHotelBookingQuery request, CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid occurred!!");

            var cach_result = await cachService.GetAsync($"My-bookings- {currentIUser.UserId}", cancellationToken);

            if (cach_result != null)
                return await Result.SuccessAsync(cach_result, "Data recieved successfully");

            var My_Bookings = await hotel_booking_instance.GetListSelectorAsync<MyHotelBookingsResponseDTO>(

                                predicate: op => op.booking.user_id == currentIUser.UserId,
                                selector: op => new MyHotelBookingsResponseDTO
                                {
                                    CheckInDate = op.check_in_date,
                                    CheckOutDate = op.check_out_date,
                                    BookingId = op.booking_id,
                                    HotelName = op.room.hotel.name,
                                    MainImage = op.room.hotel.main_image_url,
                                    Status = op.booking.status!.ToString(),
                                    TotalPrice = op.booking.total_price
                                },
                                cancellationToken,
                                includes: 
                                 op => op.booking); //. not necessary 

            await cachService.SetUserIdScopedAsync($"my-bookings- {currentIUser.UserId}",currentIUser.UserId, My_Bookings, cancellationToken);

            return await Result.SuccessAsync<List<MyHotelBookingsResponseDTO>>(My_Bookings, "Data recieved successfully. ");
        }
    }
}
