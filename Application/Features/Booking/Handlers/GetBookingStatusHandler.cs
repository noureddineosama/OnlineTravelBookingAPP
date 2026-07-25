using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Booking.DTOs;
using Application.Features.Booking.Quries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.Handlers
{
    public sealed class GetBookingStatusHandler : IRequestHandler<GetBookingStatusQuery, GenericResult<GetBookingStatusResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<GetBookingStatusResponseDTO> cachService;
        private readonly ICurrentIUserService currentIUser;

        public GetBookingStatusHandler(IUnitOfWork unitOfWork,
                                       ICachService<GetBookingStatusResponseDTO> cachService,
                                       ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<GetBookingStatusResponseDTO>> Handle(GetBookingStatusQuery request, CancellationToken cancellationToken)
        {
            var booking_instance = unitOfWork.Repository<booking>();
            if (booking_instance is null) throw new ArgumentNullException(nameof(booking_instance));

            var cach_result = await cachService.GetAsync($"Booking status with id- {request.id} result", cancellationToken);
            if (cach_result is not null)
                return await Result.SuccessAsync<GetBookingStatusResponseDTO>(cach_result, "Booing detaisl recieved successfully. ");

            var booking_status = await booking_instance.GetSelectorAsync(predicate: op => op.id == request.id,
                                                                           selector: op => new GetBookingStatusResponseDTO
                                                                           {
                                                                               BookingId = op.id,
                                                                               LastUpdated = op.updated_at,
                                                                               Status  = op.status
                                                                           },cancellationToken);
            if (booking_status is null)
                return await Result.FailureAsync<GetBookingStatusResponseDTO>("Booking not found. ");

            await cachService.SetUserIdScopedAsync($"Booking status with id- {request.id} result",currentIUser.UserId ,booking_status, cancellationToken);

            return await Result.SuccessAsync<GetBookingStatusResponseDTO>(booking_status, "Booking status recieved successfully. ");
        }
    }
}
