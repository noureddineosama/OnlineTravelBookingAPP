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
    public sealed class MyBookingsHandler : IRequestHandler<MyBookingsResponseQuery, GenericResult<List<MyBookingsResponseDTO>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;
        private readonly ICachService<List<MyBookingsResponseDTO>> cachService;

        public MyBookingsHandler(IUnitOfWork unitOfWork,
                                ICurrentIUserService currentIUser, 
                                ICachService<List<MyBookingsResponseDTO>> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
            this.cachService = cachService;
        }
        public async Task<GenericResult<List<MyBookingsResponseDTO>>> Handle(MyBookingsResponseQuery request, CancellationToken cancellationToken)
        {
            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException("Something invalid occurred!!");

            var cach_result = await cachService.GetAsync("My-bookings", cancellationToken);

            if (cach_result != null)
                return await Result.SuccessAsync(cach_result, "Data recieved successfully");

            var My_Bookings = await hotel_booking_instance.GetListSelectorAsync<MyBookingsResponseDTO>(

                                predicate: op => op.booking.user_id == currentIUser.UserId,
                                selector: op => new MyBookingsResponseDTO
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

            await cachService.SetAsync("My-bookings", My_Bookings, cancellationToken);

            return await Result.SuccessAsync<List<MyBookingsResponseDTO>>(My_Bookings);
        }
    }
}
