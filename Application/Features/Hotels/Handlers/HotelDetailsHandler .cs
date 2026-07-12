using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Hotels.DTOs;
using Application.Features.Hotels.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class HotelDetailsHandler : IRequestHandler<HotelDetailsQuery, GenericResult<HotelDetailsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;

        public HotelDetailsHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<GenericResult<HotelDetailsResponseDTO>> Handle(HotelDetailsQuery request, CancellationToken cancellationToken)
        {
            var instance = unitOfWork.Repository<hotel>();
            if(instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var result = await instance.GetSelectorAsync(predicate: op => op.id == request.Id,
                                                         selector: op => new HotelDetailsResponseDTO
                                                         {
                                                             Id = request.Id,
                                                             Description = op.description,
                                                             CheckInTime = op.check_in_time,
                                                             CheckOutTime = op.check_out_time,
                                                             MainImageUrl = op.main_image_url,
                                                             Name = op.name,
                                                             Slug = op.slug,
                                                             Status = op.status,
                                                             StarRating = op.star_rating,
                                                             Location = new LocationResponseDTO
                                                             {
                                                                 City = op.location.city,
                                                                 Country = op.location.country,
                                                                 Address = op.location.address_line,
                                                                 Id = op.location_id
                                                             },
                                                             Rooms= op.rooms.Select(op => new RoomResponsedTO
                                                             {
                                                                 AvailableRooms = op.room_availabilities.Select(op => op.available_units).Count(),
                                                                 IsAvailable = op.room_availabilities.Select(op => op.available_units).Count() < 1,
                                                                 MainImageUrl= op.hotel.main_image_url,
                                                                 MaxAdults = op.hotel_bookings.Max(op => op.guests_adults),
                                                                 MaxChildren = op.hotel_bookings.Max(op => op.guests_children),
                                                                 PricePerNight= op.price_per_night,
                                                                 RoomId = op.id,
                                                                 RoomName  = op.name
                                                             }).ToList(),
                                                             Images = op.hotel_images.Select(op => new HotelImageResponseDTO
                                                             {
                                                                 Id = op.id,
                                                                 ImageUrl = op.url
                                                             }).ToList()
                                                         });

            if (result == null)
                return await Result.FailureAsync<HotelDetailsResponseDTO>("Proccess Failed!!");

            return await Result.SuccessAsync(result, "Data Recieved Successfully");
        }
    }
}
