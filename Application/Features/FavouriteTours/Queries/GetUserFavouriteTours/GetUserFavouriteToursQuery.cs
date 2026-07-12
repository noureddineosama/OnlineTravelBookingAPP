using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Features.FavouriteTours.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.FavouriteTours.Queries.GetUserFavouriteTours;

/// <summary>
/// Paginated query to retrieve a user's favourite tours with enriched tour details.
/// </summary>
public sealed record GetUserFavouriteToursQuery : PagedQuery, IRequest<ApiResponse<PagedResult<FavouriteTourResponse>>>
{
    public long UserId { get; init; }
}

public sealed class GetUserFavouriteToursQueryHandler
    : IRequestHandler<GetUserFavouriteToursQuery, ApiResponse<PagedResult<FavouriteTourResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserFavouriteToursQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<FavouriteTourResponse>>> Handle(
        GetUserFavouriteToursQuery request, CancellationToken cancellationToken)
    {
        // Build the joined query: favorites → tours (with location + price tiers)
        var query = from fav in _context.favorites
                    join t in _context.tours.Include(t => t.location)
                                             .Include(t => t.tour_price_tiers)
                        on fav.item_id equals t.id
                    where fav.user_id == request.UserId
                       && fav.category == "tour"
                    orderby fav.added_at descending
                    select new { fav, t };

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Project to DTOs
        var dtos = items.Select(x =>
        {
            var lowestTier = x.t.tour_price_tiers
                .OrderBy(pt => pt.adult_price)
                .FirstOrDefault();

            return new FavouriteTourResponse
            {
                FavouriteId       = x.fav.id,
                TourId            = x.t.id,
                Title             = x.t.title,
                Slug              = x.t.slug,
                Summary           = x.t.summary,
                MainImageUrl      = x.t.main_image_url,
                DurationDays      = x.t.duration_days,
                LocationCity      = x.t.location?.city,
                LocationCountry   = x.t.location?.country,
                StartingFromPrice = lowestTier?.adult_price,
                Currency          = lowestTier?.currency,
                AddedAt           = x.fav.added_at
            };
        }).ToList().AsReadOnly();

        var pagedResult = new PagedResult<FavouriteTourResponse>
        {
            Items      = dtos,
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize
        };

        return ApiResponse<PagedResult<FavouriteTourResponse>>.Ok(pagedResult);
    }
}
