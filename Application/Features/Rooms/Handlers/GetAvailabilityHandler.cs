using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Passengers.Queries.GetAllPassengers;
using Application.Features.Rooms.DTOs;
using Application.Features.Rooms.Queries;
using Domain.Entities;
using MediatR;

namespace Application.Features.Rooms.Handlers
{
    public class GetAvailabilityHandler : IRequestHandler<GetAvailabilityQuery, GenericResult<GetAvailabilityResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<GetAvailabilityResponseDTO> cachService;

        public GetAvailabilityHandler(IUnitOfWork unitOfWork, 
                                      ICachService<GetAvailabilityResponseDTO> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cachService;
        }
        public async Task<GenericResult<GetAvailabilityResponseDTO>> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if (room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            var cach_result = await cachService.GetAsync($"check-availability-room with room id : {request.roomId}", cancellationToken);
            if (cach_result is not null)
                return await Result.SuccessAsync<GetAvailabilityResponseDTO>(cach_result);

            var availibility_room = await room_instance.GetSelectorAsync(predicate: op => op.id == request.roomId &&
                                                                                 op.IsDeleted == false && op.status == "Active",
                                                                     selector: op => new GetAvailabilityResponseDTO
                                                                     {
                                                                         availabilities = op.room_availabilities
                                                                                                    .OrderBy(op => op.date)
                                                                                                    .Select(op => new AvailabilityDayDTO
                                                                                                    {
                                                                                                        Date = op.date,
                                                                                                        IsAvailable = op.IsAvailable,
                                                                                                        PriceOverride = op.price_override
                                                                                                    }).ToList()
                                                                     });

            await cachService.SetAsync($"check-availability-room with room id : {request.roomId}", availibility_room, cancellationToken);

            return await Result.SuccessAsync<GetAvailabilityResponseDTO>(availibility_room);
        }
    }
}
