using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Hotels.DTOs;
using Application.Features.Hotels.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class GetHotelsHandler : IRequestHandler<GetPagedHotelsQuery, PaginatedResult<GetHotelsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;
        private readonly ILogger<GetHotelsHandler> logger;

        public GetHotelsHandler(IUnitOfWork unitOfWork, 
                                ICurrentIUserService currentIUser,
                                ILogger<GetHotelsHandler> logger)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
            this.logger = logger;
        }
        public async Task<PaginatedResult<GetHotelsResponseDTO>> Handle(GetPagedHotelsQuery request, CancellationToken cancellationToken)
        {
            var hotel_instance = unitOfWork.Repository<hotel>();
            if (hotel_instance == null)
                throw new ArgumentNullException(nameof(hotel_instance));

            var paginated_hotels_result = await hotel_instance
                .GetPaginationAsync(predicate: op => op.status == request.requestDTO.Status.ToString() &&
                                                op.location_id == request.requestDTO.LocationId &&
                                                op.star_rating == request.requestDTO.StarRating,
                                    selector: op => new GetHotelsResponseDTO
                                    {
                                        City = op.location.city,
                                        CreatedAt = op.created_at,
                                        HotelId = op.id,
                                        MainImageUrl = op.main_image_url,
                                        Name = op.name,
                                        RoomsCount = op.rooms.Count(),
                                        StarRating = op.star_rating,
                                        Slug = op.slug,
                                        Status = op.status
                                    },
                                     page: request.requestDTO.PageNumber,
                                     pageSize: request.requestDTO.PageSize,
                                     message: "Paginated data recieved successfully.",
                                     cancellationToken);

            if (paginated_hotels_result == null)
                logger.LogError("Something invalid occurred in Get Hotels Service or handler . ");

            return paginated_hotels_result;
        }
    }
}
