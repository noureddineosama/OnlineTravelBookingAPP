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
    public class SearchHandler : IRequestHandler<SearchQuery, PaginatedResult<SearchHotelResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<PaginatedResult<SearchHotelResponseDTO>> cachService;

        public SearchHandler(IUnitOfWork unitOfWork, ICachService<PaginatedResult<SearchHotelResponseDTO>> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
        }

        public async Task<PaginatedResult<SearchHotelResponseDTO>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var instance = unitOfWork.Repository<hotel>();
            if (request == null)
            {
                return new PaginatedResult<SearchHotelResponseDTO>
                {
                    IsSuccess = false,
                    message = "Request cannot be null",
                    Data = default,
                    pagination = new PaginationMetadata(),
                    TotalCount = 0
                };
            }

            var cach_result = await cachService.GetAsync($"get-hotels-" +
                                                         $"{request.requestdTO.PageNumber}-" +
                                                         $"{request.requestdTO.PageSize}-" +
                                                         $"{request.requestdTO.City}-" +
                                                         $"{request.requestdTO.StarRating}-", cancellationToken);
            if (cach_result is not null)
                return cach_result;

            var result = await instance.GetPaginationAsync(predicate: op => op.location.city == request.requestdTO.City &&
                                                                        op.location_id == request.requestdTO.location_id && 
                                                                        op.check_in_time >= request.requestdTO.CheckInDate &&
                                                                        op.check_out_time == request.requestdTO.CheckOutDate &&
                                                                        op.star_rating >= request.requestdTO.StarRating,
                                                           selector: op => new SearchHotelResponseDTO
                                                           {
                                                               hote_id = op.id,
                                                               City = op.location.city,
                                                               LowestPrice = op.rooms.Min(op => op.price_per_night),
                                                               MainImage = op.main_image_url,
                                                               hotel_name = op.name,
                                                               Slug = op.slug,
                                                               StarRating = op.star_rating,
                                                               Available = op.rooms
                                                                            .SelectMany(op => op.room_availabilities.Where(op => op.room_id == op.room_id)
                                                                            .Select(op => op.IsAvailable)).FirstOrDefault()  //. Available for room not for  hotel
                                                           },
                                                           includes: op => op.rooms,
                                                           cancellationToken: cancellationToken,
                                                           page: request.requestdTO.PageNumber,
                                                           pageSize: request.requestdTO.PageSize);
            await cachService.SetAsync($"get-hotels-" +
                                                         $"{request.requestdTO.PageNumber}-" +
                                                         $"{request.requestdTO.PageSize}-" +
                                                         $"{request.requestdTO.City}-" +
                                                         $"{request.requestdTO.StarRating}-", result, cancellationToken);

            return result;                                               
        }
    }
}
