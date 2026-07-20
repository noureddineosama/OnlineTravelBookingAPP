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
                                                             Rooms = op.rooms.Select(r => new RoomResponsedTO
                                                             {
                                                                 AvailableRooms = r.room_availabilities.Count(a => a.IsAvailable),
                                                                 IsAvailable = r.room_availabilities.Any(a => a.IsAvailable),
                                                                 MainImageUrl = r.room_images.Select(op => op.url).FirstOrDefault(),
                                                                 MaxAdults = r.occupancy_adults,
                                                                 MaxChildren = r.occupancy_children,
                                                                 PricePerNight = r.price_per_night,
                                                                 RoomId = r.id,
                                                                 RoomName = r.name
                                                             }).ToList(),
                                                             Images = op.hotel_images.Select(img => new HotelImageResponseDTO
                                                             {
                                                                 Id = img.id,
                                                                 ImageUrl = img.url
                                                             }).ToList(),

                                                         }, cancellationToken);

            if (result == null)
                return await Result.FailureAsync<HotelDetailsResponseDTO>("Validate Failed!!");

            return await Result.SuccessAsync(result, "Data Recieved Successfully");
        }
    }
}
