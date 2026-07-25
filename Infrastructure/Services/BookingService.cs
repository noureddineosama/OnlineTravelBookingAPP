using Application.Common.Interfaces;
using Application.Common.Services;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;

        public BookingService(IUnitOfWork unitOfWork,
                              ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
        }

        public async Task<booking> CreateBookingAsync(
                                                long userId,
                                                string category,
                                                decimal subtotal,
                                                decimal totalPrice,
                                                string currency,
                                                CancellationToken cancellationToken)
        {
            booking result = new booking
            {
                booking_number = BookingNumber.GeneratBookingNumber(),
                discount_amount = default,
                category = category,
                user_id = userId,
                created_at = DateTime.UtcNow,
                CreatedBy = userId,
                currency = currency,
                payment_status = "pending",
                subtotal = subtotal,
                total_price = totalPrice,
                status = BookingStatus.Pending.ToString()
            };

            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var booking_instance = unitOfWork.Repository<booking>();
            if (booking_instance == null)
                throw new ArgumentNullException(nameof(booking_instance));
            await booking_instance.AddAsync(result);

            return result;
        }

        public async Task UdpateBookingStatusAsync(long bookingId, 
                                                   string newBookingStatus,
                                                   CancellationToken cancelltionToken)
        {
            var booking_instance = unitOfWork.Repository<booking>();
            if (booking_instance is null)
                throw new ArgumentNullException(nameof(booking_instance));

            var booking = await booking_instance.GetByIdAsync(predicate:
                                                        op => op.id == bookingId &&
                                                        op.IsCancelled == false &&
                                                        op.IsDeleted == false &&
                                                        op.status != "Cancelled", cancelltionToken);
            if (booking is null)
                throw new ArgumentNullException("Booking not found. ");

            booking.updated_at = DateTime.UtcNow;
            booking.UpdatedBy = currentIUser.UserId;
            booking.status = newBookingStatus;
        }

        public async Task UpdateBookingPriceAsync(long bookingId,
                                                  decimal subTotal,
                                                  decimal TotalPrice,
                                                  CancellationToken cancellationToken)
        {
            var booking_instance = unitOfWork.Repository<booking>();
            if (booking_instance is null)
                throw new ArgumentNullException(nameof(booking_instance));

            var booking = await booking_instance.GetByIdAsync(predicate:
                                                        op => op.id == bookingId &&
                                                        op.IsCancelled == false &&
                                                        op.IsDeleted == false &&
                                                        op.status != "Cancelled", cancellationToken);
            if (booking is null)
                throw new ArgumentNullException("Booking not found. ");

            booking.subtotal = subTotal;
            booking.total_price = TotalPrice;
            booking.updated_at = DateTime.UtcNow;
            booking.UpdatedBy = currentIUser.UserId;
        }

        public async Task CancelBookingStatusAsync(long bookingId, 
                                                   CancellationReasonType reasonType,
                                                   string? details,
                                                   CancellationToken cancellationToken)
        {
            var booking_instance = unitOfWork.Repository<booking>();
            if (booking_instance is null)
                throw new ArgumentNullException(nameof(booking_instance));

            var booking = await booking_instance.GetByIdAsync(predicate:
                                                        op => op.id == bookingId &&
                                                        op.IsCancelled == false &&
                                                        op.IsDeleted == false &&
                                                        op.status != "Cancelled", cancellationToken);
            if (booking is null)
                throw new ArgumentNullException("Booking not found. ");

            booking.status = BookingStatus.Cancelled.ToString();
            booking.cancellation_reason_details = details ?? default;
            booking.IsCancelled = true;
            booking.cancellation_reason_type = reasonType;
            booking.cancelled_at = DateTime.UtcNow;
            booking.updated_at = DateTime.UtcNow;
            booking.UpdatedBy = currentIUser.UserId;

        }
    }
}
