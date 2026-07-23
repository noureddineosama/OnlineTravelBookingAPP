using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Features.Favorites.Cache;
using Application.Features.Favorites.DTOs;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Favorites.Queries.GetMyFavorites;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns a paginated list of UI-ready favourite cards for a user.
///
/// PERFORMANCE STRATEGY — avoids N+1 queries:
///   Query 1 : Fetch paginated favorite rows for this user.
///   Query 2 : Batch-load Tour details    (only if the page contains Tour favourites).
///   Query 3 : Batch-load Hotel details   (only if the page contains Hotel favourites).
///   Query 4 : Batch-load Flight details  (only if the page contains Flight favourites).
///   Query 5 : Batch-load Car details     (only if the page contains Car favourites).
///   Total   : 1 + (number of distinct categories on this page) — maximum 5.
///
/// TODO: Replace request.UserId with ICurrentUserService.UserId once JWT auth is implemented.
/// </summary>
public sealed record GetMyFavoritesQuery : PagedQuery,
    IRequest<ApiResponse<PagedResult<FavoriteDto>>>,
    ICacheableQuery
{
    /// <summary>ID of the authenticated passenger whose favourites to load.</summary>
    public long              UserId   { get; init; }
    public FavoriteCategory? Category { get; init; } // null = all categories

    // ── ICacheableQuery ──────────────────────────────────────────────────────
    /// <inheritdoc />
    /// User-scoped key prevents data leaking across different users.
    public string CacheKey =>
        FavoriteCacheKeys.UserList(UserId, Category?.ToString(), Page, PageSize);

    /// <inheritdoc />
    /// Returns <c>null</c> — driven by <c>CacheSettings.FavoritesListMinutes</c> in appsettings.json.
    public TimeSpan? SlidingExpiration => null;
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class GetMyFavoritesQueryValidator : AbstractValidator<GetMyFavoritesQuery>
{
    public GetMyFavoritesQueryValidator()
    {
        // TODO: Remove UserId validation once JWT is implemented — user will be from token.
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Category must be one of: Tour, Hotel, Flight, Car.")
            .When(x => x.Category.HasValue);
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetMyFavoritesQueryHandler
    : IRequestHandler<GetMyFavoritesQuery, ApiResponse<PagedResult<FavoriteDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetMyFavoritesQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<ApiResponse<PagedResult<FavoriteDto>>> Handle(
        GetMyFavoritesQuery request, CancellationToken cancellationToken)
    {
        // ── Step 1: Paginate the favorites table ──────────────────────────────
        // One lightweight query — only fetches the favorite rows (id, category, item_id, added_at).
        // No entity tracking needed for a read-only query.
        
        var query = _uow.Repository<favorite>().Query()
            .Where(f => f.user_id == request.UserId)
            .OrderByDescending(f => f.added_at)
            .AsNoTracking();

        if (request.Category.HasValue)
        {
            var categoryStr = request.Category.Value.ToDbString();
            query = query.Where(f => f.category == categoryStr);
        }

        var paged = await query.ToPagedResultAsync(request, cancellationToken);

        if (!paged.Items.Any())
        {
            return ApiResponse<PagedResult<FavoriteDto>>.Ok(
                paged.MapTo(_ => (IReadOnlyList<FavoriteDto>)[]));
        }

        // ── Step 2: Group item IDs by category (in-memory, list is already small) ─
        // page size is capped at 100 items, so this is always cheap.

        var tourIds   = paged.Items.Where(f => f.category == "tour")  .Select(f => (long)f.item_id).ToList();
        var hotelIds  = paged.Items.Where(f => f.category == "hotel") .Select(f => (long)f.item_id).ToList();
        var flightIds = paged.Items.Where(f => f.category == "flight").Select(f => (long)f.item_id).ToList();
        var carIds    = paged.Items.Where(f => f.category == "car")   .Select(f => (long)f.item_id).ToList();

        // ── Steps 3–6: ONE batch query per category that actually appears ─────
        // If this page has no Hotel favourites, the Hotel query is skipped entirely.

        var tours   = tourIds.Count   > 0 ? await LoadToursAsync(tourIds,   cancellationToken) : [];
        var hotels  = hotelIds.Count  > 0 ? await LoadHotelsAsync(hotelIds,  cancellationToken) : [];
        var flights = flightIds.Count > 0 ? await LoadFlightsAsync(flightIds, cancellationToken) : [];
        var cars    = carIds.Count    > 0 ? await LoadCarsAsync(carIds,     cancellationToken) : [];

        // ── Step 7: Merge — O(1) dictionary lookups, zero additional queries ──

        var dtos = paged.Items.Select(f => f.category switch
        {
            "tour"   => BuildDto(f, tours),
            "hotel"  => BuildDto(f, hotels),
            "flight" => BuildDto(f, flights),
            "car"    => BuildDto(f, cars),
            _        => BuildFallbackDto(f)   // unknown category — return base fields only
        }).ToList();

        return ApiResponse<PagedResult<FavoriteDto>>.Ok(
            paged.MapTo(_ => (IReadOnlyList<FavoriteDto>)dtos));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Batch loaders — one SQL query each, result cached in a dictionary
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<Dictionary<long, FavoriteDto>> LoadToursAsync(
        List<long> ids, CancellationToken ct)
    {
        return await _uow.Repository<tour>().Query()
            .Where(t => ids.Contains(t.id) && !t.is_deleted && t.status == Domain.Enums.TourStatus.Active)
            .AsNoTracking()
            .Select(t => new FavoriteDto
            {
                Category      = FavoriteCategory.Tour,
                CategoryLabel = "Tour",
                ItemId        = t.id,
                Title         = t.title,
                Subtitle      = t.summary,
                ImageUrl      = t.main_image_url,
                Price         = t.tour_price_tiers
                                 .OrderBy(p => p.adult_price)
                                 .Select(p => (decimal?)p.adult_price)
                                 .FirstOrDefault(),
                Currency      = t.tour_price_tiers
                                 .OrderBy(p => p.adult_price)
                                 .Select(p => p.currency)
                                 .FirstOrDefault(),
                Rating        = (double?)_uow.Repository<review>().Query()
                                 .Where(r => r.category == "tour"
                                          && r.item_id  == t.id
                                          && r.status   == "approved")
                                 .Average(r => (double?)r.rating),
                Location      = t.location != null
                                 ? t.location.city + ", " + t.location.country
                                 : null,
                BadgeText     = t.duration_days != null
                                 ? t.duration_days.ToString() + " Days"
                                 : null
            })
            .ToDictionaryAsync(d => d.ItemId, ct);
    }

    private async Task<Dictionary<long, FavoriteDto>> LoadHotelsAsync(
        List<long> ids, CancellationToken ct)
    {
        return await _uow.Repository<hotel>().Query()
            .Where(h => ids.Contains(h.id))
            .AsNoTracking()
            .Select(h => new FavoriteDto
            {
                Category      = FavoriteCategory.Hotel,
                CategoryLabel = "Hotel",
                ItemId        = h.id,
                Title         = h.name,
                Subtitle      = h.description,
                ImageUrl      = h.main_image_url,
                Price         = h.rooms
                                 .Where(r => r.status == "active")
                                 .Select(r => (decimal?)r.price_per_night)
                                 .Min(),
                Currency      = null, // currency not stored on hotel/room entities
                Rating        = (double?)_uow.Repository<review>().Query()
                                 .Where(r => r.category == "hotel"
                                          && r.item_id  == h.id
                                          && r.status   == "approved")
                                 .Average(r => (double?)r.rating),
                Location      = h.location != null
                                 ? h.location.city + ", " + h.location.country
                                 : null,
                BadgeText     = h.star_rating != null
                                 ? h.star_rating.ToString() + "★"
                                 : null
            })
            .ToDictionaryAsync(d => d.ItemId, ct);
    }

    private async Task<Dictionary<long, FavoriteDto>> LoadFlightsAsync(
        List<long> ids, CancellationToken ct)
    {
        return await _uow.Repository<flight>().Query()
            .Where(f => ids.Contains(f.id))
            .AsNoTracking()
            .Select(f => new FavoriteDto
            {
                Category      = FavoriteCategory.Flight,
                CategoryLabel = "Flight",
                ItemId        = f.id,
                Title         = f.flight_number + " · "
                              + f.origin_city + " → " + f.destination_city,
                Subtitle      = f.carrier_name + " · " + f.cabin_class,
                ImageUrl      = null,
                Price         = f.base_price,
                Currency      = f.currency,
                Rating        = (double?)_uow.Repository<review>().Query()
                                 .Where(r => r.category == "flight"
                                          && r.item_id  == f.id
                                          && r.status   == "approved")
                                 .Average(r => (double?)r.rating),
                Location      = null, // flight uses BadgeText for route
                BadgeText     = f.origin_airport_code + "→" + f.destination_airport_code
            })
            .ToDictionaryAsync(d => d.ItemId, ct);
    }

    private async Task<Dictionary<long, FavoriteDto>> LoadCarsAsync(
        List<long> ids, CancellationToken ct)
    {
        return await _uow.Repository<car>().Query()
            .Where(c => ids.Contains(c.id))
            .AsNoTracking()
            .Select(c => new FavoriteDto
            {
                Category      = FavoriteCategory.Car,
                CategoryLabel = "Car",
                ItemId        = c.id,
                Title         = (c.brand != null ? c.brand.name + " " : "") + c.model,
                Subtitle      = c.transmission + " · " + c.seats_count + " seats",
                ImageUrl      = c.car_images
                                 .OrderBy(ci => ci.sort_order)
                                 .Select(ci => ci.url)
                                 .FirstOrDefault(),
                Price         = c.car_pricing_tiers
                                 .OrderBy(p => p.price_per_hour)
                                 .Select(p => (decimal?)p.price_per_hour)
                                 .FirstOrDefault(),
                Currency      = null, // currency not stored on car entity
                Rating        = (double?)_uow.Repository<review>().Query()
                                 .Where(r => r.category == "car"
                                          && r.item_id  == c.id
                                          && r.status   == "approved")
                                 .Average(r => (double?)r.rating),
                Location      = null,
                BadgeText     = c.car_category != null ? c.car_category.name : null
            })
            .ToDictionaryAsync(d => d.ItemId, ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Merges the base favorite row fields (FavoriteId, UserId, AddedAt) with the
    /// pre-loaded item projection. Dictionary lookup is O(1).
    /// </summary>
    private static FavoriteDto BuildDto(
        Domain.Entities.favorite fav,
        Dictionary<long, FavoriteDto> lookup)
    {
        if (!lookup.TryGetValue(fav.item_id, out var p))
            return BuildFallbackDto(fav); // item deleted after favouriting

        // Copy all display fields from the projection and set the base
        // favorite fields (id, userId, addedAt) from the favorite row.
        return new FavoriteDto
        {
            FavoriteId    = fav.id,
            UserId        = fav.user_id,
            AddedAt       = fav.added_at,
            Category      = p.Category,
            CategoryLabel = p.CategoryLabel,
            ItemId        = p.ItemId,
            Title         = p.Title,
            Subtitle      = p.Subtitle,
            ImageUrl      = p.ImageUrl,
            Price         = p.Price,
            Currency      = p.Currency,
            Rating        = p.Rating,
            Location      = p.Location,
            BadgeText     = p.BadgeText
        };
    }

    /// <summary>
    /// Returns a minimal DTO when the referenced item no longer exists.
    /// Prevents the whole page from failing because one item was deleted.
    /// </summary>
    private static FavoriteDto BuildFallbackDto(Domain.Entities.favorite fav)
        => new()
        {
            FavoriteId    = fav.id,
            UserId        = fav.user_id,
            Category      = Enum.TryParse<FavoriteCategory>(fav.category, ignoreCase: true, out var cat)
                                ? cat : default,
            CategoryLabel = fav.category,
            ItemId        = fav.item_id,
            AddedAt       = fav.added_at,
            Title         = "[Item no longer available]"
        };
}
