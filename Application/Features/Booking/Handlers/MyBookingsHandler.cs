using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Booking.DTOs;
using Application.Features.Booking.Quries;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.Handlers
{
    public sealed class MyBookingsHandler : IRequestHandler<MyBookingsQuery, PaginatedResult<MyBookingsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;
        private readonly ICachService<PaginatedResult<MyBookingsResponseDTO>> cachService;

        public MyBookingsHandler(IUnitOfWork unitOfWork, 
                                 ICurrentIUserService currentIUser, 
                                 ICachService<PaginatedResult<MyBookingsResponseDTO>> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
            this.cachService = cachService;
        }
        public async Task<PaginatedResult<MyBookingsResponseDTO>> Handle(MyBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookings_instance = unitOfWork.Repository<booking>();
            if (bookings_instance is null) throw new ArgumentNullException(nameof(bookings_instance));

            //var cach_result = cachService.GetAsync($"my-bookings:{currentIUser.UserId}:page:{request.page}:size:{request.pageSize }", cancellationToken);
            //if (cach_result is not null)
            //    return await cach_result;

            var paged_result = await bookings_instance
                .GetPaginationAsync(predicate: book => book.user_id == currentIUser.UserId &&
                                                          book.status == "Pending" &&
                                                        book.IsDeleted == false &&
                                                        book.IsCancelled == false ,
                                    selector: op => new MyBookingsResponseDTO
                                    {
                                        BookingId = op.id,
                                        Category = op.category,
                                        Title  = op.car_booking.car.brand.name ?? 
                                                        op.flight_booking.flight.destination_city ?? 
                                                        op.tour_booking.tour_schedule.tour.title ?? 
                                                        op.hotel_booking.room.hotel.name,
                                        CreatedAt = op.created_at,
                                        Currency = op.currency,
                                        Status = op.status,
                                        TotalPrice = op.total_price
                                    },
                                    page: request.page,
                                    pageSize: request.pageSize,
                                    message: "Data recieved successfully. ",
                                    cancellationToken: cancellationToken,
                                    op => op.flight_booking.flight ,
                                    op => op.flight_booking ,
                                    op => op.tour_booking.tour_schedule.tour,
                                    op => op.tour_booking.tour_schedule,
                                    op => op.hotel_booking.room, 
                                    op => op.hotel_booking,
                                    op => op.hotel_booking.room.hotel);

            await cachService.SetUserIdScopedAsync($"my-bookings:{currentIUser.UserId}:page:{request.page}:size:{request.pageSize}",currentIUser.UserId ,paged_result, cancellationToken);

            return paged_result;
        }
    }
}
