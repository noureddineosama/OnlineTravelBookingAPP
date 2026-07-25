using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Booking.DTOs;
using Application.Features.Booking.Quries;
using Domain.Entities;
using MediatR;
using Stripe.V2.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.Handlers
{
    public sealed class BookingDetailsHandler : IRequestHandler<BookingDetailsQuery, GenericResult<BookingDetailsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<BookingDetailsResponseDTO> cachService;
        private readonly ICurrentIUserService currentIUser;

        public BookingDetailsHandler(IUnitOfWork unitOfWork,
                                     ICachService<BookingDetailsResponseDTO> cachService,
                                     ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<BookingDetailsResponseDTO>> Handle(BookingDetailsQuery request, CancellationToken cancellationToken)
        {
            var bookings_instance = unitOfWork.Repository<booking>();
            if (bookings_instance is null) throw new ArgumentNullException(nameof(bookings_instance));

            var cach_result = await cachService.GetAsync($"Booking - {request.bookingId} result", cancellationToken);
            if(cach_result is not null )
                return await Result.SuccessAsync<BookingDetailsResponseDTO>(cach_result, "Booing detaisl recieved successfully. ");

            var result = await bookings_instance
                .GetSelectorAsync(predicate: op => op.id == request.bookingId &&
                                                   op.IsDeleted == false &&
                                                   op.IsCancelled == false,
                                 selector: op => new BookingDetailsResponseDTO
                                 {
                                     BookingId = op.id,
                                     Category = op.category,
                                     CreatedAt = op.created_at,
                                     Currency = op.currency,
                                     Status = op.status,
                                     SubTotal = op.subtotal,
                                     TotalPrice = op.total_price,
                                     Details = op.cancellation_reason_details ?? default
                                 }, cancellationToken);
            if (result == null)
                return await Result.FailureAsync<BookingDetailsResponseDTO>("Booking not found. ");

            await cachService.SetUserIdScopedAsync($"Booking - {request.bookingId} result",currentIUser.UserId ,result, cancellationToken);

            return await Result.SuccessAsync<BookingDetailsResponseDTO>(result, "Booing detaisl recieved successfully. ");
        }
    }
}