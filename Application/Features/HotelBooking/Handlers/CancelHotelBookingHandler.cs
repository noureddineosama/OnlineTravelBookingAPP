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
using System.Diagnostics;
using Stripe;

namespace Application.Features.HotelBooking.Handlers
{
    public sealed class CancelHotelBookingHandler : IRequestHandler<CancelHotelBookingQuery, GenericResult<CancelHotelBookingResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;

        public CancelHotelBookingHandler(IUnitOfWork unitOfWork, ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
        }
        //. Cancelling the booking using the id for the hotel booking with steps: 
        /// <summary>
        /// first, bringing the hotel booking using its id 
        /// after that Updating the room Availability to be is available to another booking ,
        /// the thrid step is updating the booking table with two properties or more 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>CancelHotelBookingResponseDTO</returns>
        public async Task<GenericResult<CancelHotelBookingResponseDTO>> Handle(CancelHotelBookingQuery request, CancellationToken cancellationToken)
        {

            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if(hotel_booking_instance == null)throw new ArgumentNullException(nameof(hotel_booking_instance));

            var existing_hotel_booking = await hotel_booking_instance.GetByIdAsync(predicate: op => op.id == request.id &&
                                                                                              op.booking.status == "Active" &&
                                                                                              op.booking.IsDeleted == false &&
                                                                                              op.booking.IsCancelled == false, cancellationToken);
            if (existing_hotel_booking is null)
                return await Result.FailureAsync<CancelHotelBookingResponseDTO>("Booking not found to cancel. ");

            //. Execute Update for the is avalable raw that existing in the room availability rooms 
            //. note : this execute updating process didn't use the change tracker 
            var execute_update_result_room_Available = await unitOfWork.hotelBookingRepository
                                                    .ExecuteUpdateAsync(existing_hotel_booking.room_id, existing_hotel_booking.check_in_date,
                                                                            existing_hotel_booking.check_out_date,
                                                                            cancellationToken);

            if (execute_update_result_room_Available == default)
                return await Result.FailureAsync<CancelHotelBookingResponseDTO>("Validation failed. ");

            existing_hotel_booking.booking.status = BookingStatus.Cancelled.ToString();
            existing_hotel_booking.booking.IsCancelled = true;
            existing_hotel_booking.booking.updated_at = DateTime.UtcNow;
            existing_hotel_booking.booking.UpdatedBy = currentIUser.UserId;


            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<CancelHotelBookingResponseDTO>(new CancelHotelBookingResponseDTO
            {
                Success = true,
                Message = "Booking had been cancelled"
            });
        }
    }
}
