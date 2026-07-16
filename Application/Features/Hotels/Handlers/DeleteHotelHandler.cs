using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Hotels.Commands;
using Application.Features.Hotels.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Application.Features.Hotels.Handlers
{
    public sealed class DeleteHotelHandler : IRequestHandler<DeleteHotelCommand, GenericResult<DeleteHotelResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;

        public DeleteHotelHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<GenericResult<DeleteHotelResponseDTO>> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
        {
            var hotel_instance = unitOfWork.Repository<hotel>();
            if (hotel_instance == null)
                throw new ArgumentNullException(nameof(hotel_instance));

            var hotel_booking_instance = unitOfWork.Repository<hotel_booking>();
            if (hotel_booking_instance == null)
                throw new ArgumentNullException(nameof(hotel_booking_instance));

            var existing_hotel = await hotel_instance.GetByIdAsync(op => op.id == request.id);
            if (existing_hotel == null)
                return await Result.FailureAsync<DeleteHotelResponseDTO>("Hotel not found. ");

            //. checking if there are bookings in the same time of today (if there we will not allow the remove process ) 
            if (await hotel_booking_instance
                .AnyAsync(predicate: op => op.room.hotel.id == request.id &&
                                          op.check_out_date >= DateOnly.FromDateTime(DateTime.Now), cancellationToken))
                return await Result.FailureAsync<DeleteHotelResponseDTO>("Can't remove the hotel. ");

            existing_hotel.status = hotel_Status.InActive.ToString();
            existing_hotel.IsDeleted = true;
            existing_hotel.DeletedAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync();
            return await Result.SuccessAsync<DeleteHotelResponseDTO>(new DeleteHotelResponseDTO
            {
                HotelId = existing_hotel.id,
                Success = true,
                Message = "Hotel deleted successfully"
            });
        }
    }
}