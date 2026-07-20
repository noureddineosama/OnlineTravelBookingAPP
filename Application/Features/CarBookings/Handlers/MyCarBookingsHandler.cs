using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.CarBookings.DTOs;
using Application.Features.CarBookings.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CarBookings.Handlers
{
    public sealed class MyCarBookingsHandler : IRequestHandler<MyCarBookingsQuery, GenericResult<List<MyCarBookingsResponseDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentIUserService _currentIUser;
        private readonly HybridCache _cache;

        public MyCarBookingsHandler(
            IUnitOfWork unitOfWork,
            ICurrentIUserService currentIUser,
            HybridCache cache)
        {
            _unitOfWork   = unitOfWork;
            _currentIUser = currentIUser;
            _cache        = cache;
        }

        public async Task<GenericResult<List<MyCarBookingsResponseDTO>>> Handle(MyCarBookingsQuery request, CancellationToken cancellationToken)
        {
            var userId   = _currentIUser.UserId;
            var cacheKey = $"my-car-bookings:{userId}";

            var car_bookings = await _cache.GetOrCreateAsync(
                cacheKey,
                async ct => await FetchMyBookings(userId, ct),
                cancellationToken: cancellationToken
            );

            return await Result.SuccessAsync(car_bookings);
        }

        private async Task<List<MyCarBookingsResponseDTO>> FetchMyBookings(long userId, CancellationToken ct)
        {
            var car_booking_instance = _unitOfWork.Repository<car_booking>();
            if (car_booking_instance == null)
                throw new ArgumentNullException("Something invalid occurred!!");

            return await car_booking_instance.GetListSelectorAsync<MyCarBookingsResponseDTO>(
                predicate: op => op.booking.user_id == userId,
                selector: op => new MyCarBookingsResponseDTO
                {
                    BookingId       = op.booking_id,
                    BookingNumber   = op.booking.booking_number,
                    CarModel        = op.car.model,
                    CarBrand        = op.car.brand.name,
                    MainImageUrl    = op.car.car_images.OrderBy(img => img.sort_order).Select(img => img.url).FirstOrDefault() ?? string.Empty,
                    PickupAt        = op.pickup_at,
                    DropoffAt       = op.dropoff_at,
                    PickupLocation  = (op.pickup_location.city ?? string.Empty) + ", " + (op.pickup_location.country ?? string.Empty),
                    DropoffLocation = (op.dropoff_location.city ?? string.Empty) + ", " + (op.dropoff_location.country ?? string.Empty),
                    TotalPrice      = op.booking.total_price,
                    Status          = op.booking.status!.ToString()
                },
                ct,
                includes: new Expression<Func<car_booking, object?>>[]
                {
                    op => op.booking,
                    op => op.car,
                    op => op.car.brand,
                    op => op.car.car_images,
                    op => op.pickup_location,
                    op => op.dropoff_location
                }
            );
        }
    }
}
