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
        private readonly ICachService<HotelDetailsResponseDTO> cachService;

        public HotelDetailsHandler(IUnitOfWork unitOfWork, 
                                   ICachService<HotelDetailsResponseDTO> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
        }

        public async Task<GenericResult<HotelDetailsResponseDTO>> Handle(HotelDetailsQuery request, CancellationToken cancellationToken)
        {
            var instance = unitOfWork.Repository<hotel>();
            if(instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var cach_result = await cachService.GetAsync($"hotel-details-response with id: {request.Id}", cancellationToken);
            if (cach_result != null)
                return await Result.SuccessAsync<HotelDetailsResponseDTO>(cach_result);

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
                                                                 IsAvailable = r.room_availabilities.Where(op => op.room_id == op.room_id).Select(op => op.IsAvailable).FirstOrDefault(),
                                                                 MainImageUrl = r.room_images.Select(op => op.url).FirstOrDefault(),
                                                                 MaxAdults = r.hotel_bookings.Any()
                                                                                         ? r.hotel_bookings.Max(x => x.guests_adults)
                                                                                         : r.hotel_bookings.Select(op => op.guests_adults).FirstOrDefault(),
                                                                 MaxChildren = r.hotel_bookings.Any()
                                                                                                ?r.hotel_bookings.Max(op => op.guests_children)
                                                                                                : r.hotel_bookings.Select(op => op.guests_children).FirstOrDefault()
                                                                 ,
                                                                 PricePerNight = r.price_per_night,
                                                                 RoomId = r.id,
                                                                 RoomName = r.name
                                                             }).ToList(),
                                                             Images = op.hotel_images.Select(op => new HotelImageResponseDTO
                                                             {
                                                                 Id = op.id,
                                                                 ImageUrl = op.url
                                                             }).ToList(),

                                                         }, cancellationToken);

            await cachService.SetAsync("hotel-details-response", result, cancellationToken);

            if (result == null)
                return await Result.FailureAsync<HotelDetailsResponseDTO>("Validate Failed!!");

            return await Result.SuccessAsync(result, "Data Recieved Successfully");
        }
    }
}
