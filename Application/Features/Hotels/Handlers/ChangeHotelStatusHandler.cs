using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Hotels.Commands;
using Application.Features.Hotels.DTOs;
using Domain.Entities;
using MediatR;
using MediatR.Wrappers;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class ChangeHotelStatusHandler : IRequestHandler<ChangeHotelStatusCommand, GenericResult<ChangeHotelStatusResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;

        public ChangeHotelStatusHandler(IUnitOfWork unitOfWork,
                                        ICurrentIUserService currentIUser)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
        }
        public async Task<GenericResult<ChangeHotelStatusResponseDTO>> Handle(ChangeHotelStatusCommand request, CancellationToken cancellationToken)
        {
            var hotel_instance = unitOfWork.Repository<hotel>();
            if (hotel_instance == null)
                throw new ArgumentNullException(nameof(hotel_instance));

            var hotel = await hotel_instance.GetByIdAsync(op => op.id == request.requestDTO.HotelId, cancellationToken);
            if (hotel == null)
                return await Result.FailureAsync<ChangeHotelStatusResponseDTO>("Hotel not found. ");

            var response = new ChangeHotelStatusResponseDTO
            {
                HotelId = hotel.id,
                NewStatus = request.requestDTO.Status.ToString(),
                OldStatus = hotel.status,
                Message = "Status had been changed successfully. "
            };

            hotel.status = request.requestDTO.Status.ToString();
            hotel.updated_at = DateTime.UtcNow;
            hotel.UpdatedBy = currentIUser.UserId;

            //. will update the updates in db if the process would be tracked only
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return await Result.SuccessAsync<ChangeHotelStatusResponseDTO>(response);
        }
    }
}
