using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Booking.Commands;
using Application.Features.Booking.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.Handlers
{
    public sealed class CancelBookingHandler : IRequestHandler<CancelBookingCommand, GenericResult<CancelBookingResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;

        public CancelBookingHandler(IUnitOfWork unitOfWork, 
                                    ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
        }
        async Task<GenericResult<CancelBookingResponseDTO>> IRequestHandler<CancelBookingCommand, GenericResult<CancelBookingResponseDTO>>.Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking_instance = unitOfWork.Repository<booking>();
            if (booking_instance is null) throw new ArgumentNullException(nameof(booking_instance));

            var existing_booking = await booking_instance.GetByIdAsync(predicate: op => op.id == request.bookingId &&
                                                                                   op.IsDeleted == false &&
                                                                                   op.IsCancelled == false &&
                                                                                   op.status != "Cancelled", cancellationToken);
            if (existing_booking is null) return await Result.FailureAsync<CancelBookingResponseDTO>("Booking not found. ");

            existing_booking.status = BookingStatus.Cancelled.ToString();
            existing_booking.IsCancelled = true;
            existing_booking.cancelled_at = DateTime.UtcNow;
            existing_booking.updated_at = DateTime.UtcNow;
            existing_booking.UpdatedBy = currentIUser.UserId;
            existing_booking.cancellation_reason_details = request.requestDTO.CancellationReason;
            existing_booking.cancellation_reason_type = request.requestDTO.cancellationReasonType;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<CancelBookingResponseDTO>(new CancelBookingResponseDTO
            {
                BookingId = existing_booking.id,
                CancelledAt = DateTime.Now,
                RefundAmount = existing_booking.total_price,
                Status = existing_booking.status
            });
        }
    }
}
