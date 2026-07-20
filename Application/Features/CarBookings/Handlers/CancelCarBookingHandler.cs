using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.CarBookings.Commands;
using Application.Features.CarBookings.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CarBookings.Handlers
{
    public sealed class CancelCarBookingHandler 
        : IRequestHandler<CancelCarBookingCommand, GenericResult<CancelCarBookingResponseDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentIUserService _currentIUser;
        private readonly HybridCache _cache;

        public CancelCarBookingHandler(
            IUnitOfWork unitOfWork,
            ICurrentIUserService currentIUser,
            HybridCache cache)
        {
            _unitOfWork   = unitOfWork;
            _currentIUser = currentIUser;
            _cache        = cache;
        }

        public async Task<GenericResult<CancelCarBookingResponseDTO>> Handle(
            CancelCarBookingCommand request, CancellationToken cancellationToken)
        {
            var parentBooking = await _unitOfWork.Repository<booking>()
                .Query()
                .FirstOrDefaultAsync(b =>
                    b.id == request.id &&
                    b.user_id == _currentIUser.UserId &&
                    b.category == "car",
                    cancellationToken);

            if (parentBooking is null)
                return await Result.FailureAsync<CancelCarBookingResponseDTO>(
                    $"Car booking with ID '{request.id}' was not found for this user.");

            if (parentBooking.status == BookingStatus.Cancelled.ToString())
                return await Result.FailureAsync<CancelCarBookingResponseDTO>(
                    "This booking is already cancelled.");

            parentBooking.status = BookingStatus.Cancelled.ToString();
            parentBooking.IsCancelled = true;
            parentBooking.updated_at = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Evict stale cache entries so both the details and the user's list
            // reflect the updated Cancelled status on the next fetch.
            await _cache.RemoveAsync($"car-booking-details:{request.id}", cancellationToken);
            await _cache.RemoveAsync($"my-car-bookings:{_currentIUser.UserId}", cancellationToken);

            return await Result.SuccessAsync<CancelCarBookingResponseDTO>(new CancelCarBookingResponseDTO
            {
                Success = true,
                Message = "Booking had been cancelled successfully"
            });
        }
    }
}
