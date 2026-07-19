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

            var result = await instance.GetSelectorAsync(predicate: op => op.id == request.Id && op.IsDeleted == false &&
                                                                     op.status == "Active",
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
                                                             Rooms = op.rooms.Select(op => new RoomResponsedTO
                                                             {
                                                                 IsAvailable = op.room_availabilities.Where(op => op.room_id == op.room_id).Select(op => op.IsAvailable).FirstOrDefault(),
                                                                 MainImageUrl = op.hotel.main_image_url,
                                                                 MaxAdults = op.hotel_bookings.Any()
                                                                                         ? op.hotel_bookings.Max(x => x.guests_adults)
                                                                                         : op.hotel_bookings.Select(op => op.guests_adults).FirstOrDefault(),
                                                                 MaxChildren = op.hotel_bookings.Any()
                                                                                                ? op.hotel_bookings.Max(op => op.guests_children)
                                                                                                : op.hotel_bookings.Select(op => op.guests_children).FirstOrDefault()
                                                                 ,
                                                                 PricePerNight = op.price_per_night,
                                                                 RoomId = op.id,
                                                                 RoomName = op.name
                                                             }).ToList(),
                                                             Images = op.hotel_images.Select(op => new HotelImageResponseDTO
                                                             {
                                                                 Id = op.id,
                                                                 ImageUrl = op.url
                                                             }).ToList(),

                                                         }, cancellationToken);

            if (result == null)
                return await Result.FailureAsync<HotelDetailsResponseDTO>("Validate Failed!!");

            return await Result.SuccessAsync(result, "Data Recieved Successfully");
        }
    }
}
