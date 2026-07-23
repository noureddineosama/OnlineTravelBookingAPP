using Application.Common.Caching;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Favorites.Cache;
using Application.Features.Favorites.DTOs;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Favorites.Commands.AddFavorite;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record AddFavoriteCommand(
    long             UserId,
    FavoriteCategory Category,
    long             ItemId
) : IRequest<ApiResponse<FavoriteDto>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    public AddFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be one of: Tour, Hotel, Flight, Car.");

        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("ItemId must be a valid ID.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class AddFavoriteCommandHandler
    : IRequestHandler<AddFavoriteCommand, ApiResponse<FavoriteDto>>
{
    private readonly IUnitOfWork   _uow;
    private readonly ICacheService _cache;

    public AddFavoriteCommandHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow   = uow;
        _cache = cache;
    }

    public async Task<ApiResponse<FavoriteDto>> Handle(
        AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var categoryStr = request.Category.ToDbString();

        // 1. Verify the referenced item exists and is active
        await ValidateItemAsync(request.Category, request.ItemId, cancellationToken);

        // 2. Check for duplicate (DB unique index UQ_favorites is the real safety net)
        var alreadyExists = await _uow.Repository<favorite>()
            .AnyAsync(f =>
                f.user_id  == request.UserId &&
                f.category == categoryStr    &&
                f.item_id  == request.ItemId,
                cancellationToken);

        if (alreadyExists)
            throw new ConflictException(
                $"This {request.Category} is already in your favourites.");

        // 3. Persist
        var entity = new favorite
        {
            user_id  = request.UserId,
            category = categoryStr,
            item_id  = request.ItemId,
            added_at = DateTime.UtcNow
        };

        await _uow.Repository<favorite>().AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Enrich with display fields so the frontend can render the card immediately.
        var dto = await EnrichAsync(entity, request.Category, cancellationToken);

        // Invalidate user's favorites list (all pages) and the check-key for this item.
        await _cache.RemoveByPrefixAsync(FavoriteCacheKeys.UserPrefix(request.UserId), cancellationToken);
        await _cache.RemoveAsync(
            FavoriteCacheKeys.Check(request.UserId, categoryStr, request.ItemId),
            cancellationToken);

        return ApiResponse<FavoriteDto>.Ok(dto,
            $"{request.Category} added to favourites successfully.");
    }

    // ── Item validation ───────────────────────────────────────────────────────

    private async Task ValidateItemAsync(
        FavoriteCategory category, long itemId, CancellationToken ct)
    {
        var exists = category switch
        {
            FavoriteCategory.Tour   => await _uow.Repository<tour>().AnyAsync(t => t.id == itemId && t.status == Domain.Enums.TourStatus.Active && !t.is_deleted, ct),
            FavoriteCategory.Hotel  => await _uow.Repository<hotel>().AnyAsync(h => h.id == itemId && h.status == "active", ct),
            FavoriteCategory.Flight => await _uow.Repository<flight>().AnyAsync(f => f.id == itemId && f.status == "active", ct),
            FavoriteCategory.Car    => await _uow.Repository<car>().AnyAsync(c => c.id == itemId && c.status == "active", ct),
            _ => throw new BadRequestException(
                    $"No item validation is registered for category '{category}'. " +
                    "Add a case to ValidateItemAsync.")
        };

        if (!exists)
            throw new NotFoundException($"Active {category}", itemId);
    }

    // ── Post-save enrichment (one query per category) ─────────────────────────
    // Mirrors the same projections as GetMyFavoritesQuery but for a single item.

    private async Task<FavoriteDto> EnrichAsync(
        favorite entity, FavoriteCategory category, CancellationToken ct)
        => category switch
        {
            FavoriteCategory.Tour   => await EnrichTourAsync(entity, ct),
            FavoriteCategory.Hotel  => await EnrichHotelAsync(entity, ct),
            FavoriteCategory.Flight => await EnrichFlightAsync(entity, ct),
            FavoriteCategory.Car    => await EnrichCarAsync(entity, ct),
            _                       => FallbackDto(entity, category)
        };

    private async Task<FavoriteDto> EnrichTourAsync(favorite entity, CancellationToken ct)
    {
        var t = await _uow.Repository<tour>().Query()
            .Where(t => t.id == entity.item_id)
            .AsNoTracking()
            .Select(t => new
            {
                t.title, t.summary, t.main_image_url, t.duration_days,
                Price    = t.tour_price_tiers.OrderBy(p => p.adult_price).Select(p => (decimal?)p.adult_price).FirstOrDefault(),
                Currency = t.tour_price_tiers.OrderBy(p => p.adult_price).Select(p => p.currency).FirstOrDefault(),
                Location = t.location != null ? t.location.city + ", " + t.location.country : null,
                Rating   = (double?)_uow.Repository<review>().Query().Where(r => r.category == "tour" && r.item_id == t.id && r.status == "approved").Average(r => (double?)r.rating)
            })
            .FirstOrDefaultAsync(ct);

        if (t is null) return FallbackDto(entity, FavoriteCategory.Tour);

        return new FavoriteDto
        {
            FavoriteId    = entity.id,
            UserId        = entity.user_id,
            Category      = FavoriteCategory.Tour,
            CategoryLabel = "Tour",
            ItemId        = entity.item_id,
            AddedAt       = entity.added_at,
            Title         = t.title,
            Subtitle      = t.summary,
            ImageUrl      = t.main_image_url,
            Price         = t.Price,
            Currency      = t.Currency,
            Rating        = t.Rating,
            Location      = t.Location,
            BadgeText     = t.duration_days != null ? t.duration_days + " Days" : null
        };
    }

    private async Task<FavoriteDto> EnrichHotelAsync(favorite entity, CancellationToken ct)
    {
        var h = await _uow.Repository<hotel>().Query()
            .Where(h => h.id == entity.item_id)
            .AsNoTracking()
            .Select(h => new
            {
                h.name, h.description, h.main_image_url, h.star_rating,
                Price    = h.rooms.Where(r => r.status == "active").Select(r => (decimal?)r.price_per_night).Min(),
                Location = h.location != null ? h.location.city + ", " + h.location.country : null,
                Rating   = (double?)_uow.Repository<review>().Query().Where(r => r.category == "hotel" && r.item_id == h.id && r.status == "approved").Average(r => (double?)r.rating)
            })
            .FirstOrDefaultAsync(ct);

        if (h is null) return FallbackDto(entity, FavoriteCategory.Hotel);

        return new FavoriteDto
        {
            FavoriteId    = entity.id,
            UserId        = entity.user_id,
            Category      = FavoriteCategory.Hotel,
            CategoryLabel = "Hotel",
            ItemId        = entity.item_id,
            AddedAt       = entity.added_at,
            Title         = h.name,
            Subtitle      = h.description,
            ImageUrl      = h.main_image_url,
            Price         = h.Price,
            Rating        = h.Rating,
            Location      = h.Location,
            BadgeText     = h.star_rating != null ? h.star_rating + "★" : null
        };
    }

    private async Task<FavoriteDto> EnrichFlightAsync(favorite entity, CancellationToken ct)
    {
        var f = await _uow.Repository<flight>().Query()
            .Where(f => f.id == entity.item_id)
            .AsNoTracking()
            .Select(f => new
            {
                f.flight_number, f.origin_city, f.destination_city,
                f.carrier_name, f.cabin_class,
                f.origin_airport_code, f.destination_airport_code,
                f.base_price, f.currency,
                Rating = (double?)_uow.Repository<review>().Query().Where(r => r.category == "flight" && r.item_id == f.id && r.status == "approved").Average(r => (double?)r.rating)
            })
            .FirstOrDefaultAsync(ct);

        if (f is null) return FallbackDto(entity, FavoriteCategory.Flight);

        return new FavoriteDto
        {
            FavoriteId    = entity.id,
            UserId        = entity.user_id,
            Category      = FavoriteCategory.Flight,
            CategoryLabel = "Flight",
            ItemId        = entity.item_id,
            AddedAt       = entity.added_at,
            Title         = $"{f.flight_number} · {f.origin_city} → {f.destination_city}",
            Subtitle      = $"{f.carrier_name} · {f.cabin_class}",
            Price         = f.base_price,
            Currency      = f.currency,
            Rating        = f.Rating,
            BadgeText     = $"{f.origin_airport_code}→{f.destination_airport_code}"
        };
    }

    private async Task<FavoriteDto> EnrichCarAsync(favorite entity, CancellationToken ct)
    {
        var c = await _uow.Repository<car>().Query()
            .Where(c => c.id == entity.item_id)
            .AsNoTracking()
            .Select(c => new
            {
                BrandName    = c.brand != null ? c.brand.name : null,
                c.model, c.transmission, c.seats_count,
                CategoryName = c.car_category != null ? c.car_category.name : null,
                ImageUrl     = c.car_images.OrderBy(ci => ci.sort_order).Select(ci => ci.url).FirstOrDefault(),
                Price        = c.car_pricing_tiers.OrderBy(p => p.price_per_hour).Select(p => (decimal?)p.price_per_hour).FirstOrDefault(),
                Rating       = (double?)_uow.Repository<review>().Query().Where(r => r.category == "car" && r.item_id == c.id && r.status == "approved").Average(r => (double?)r.rating)
            })
            .FirstOrDefaultAsync(ct);

        if (c is null) return FallbackDto(entity, FavoriteCategory.Car);

        return new FavoriteDto
        {
            FavoriteId    = entity.id,
            UserId        = entity.user_id,
            Category      = FavoriteCategory.Car,
            CategoryLabel = "Car",
            ItemId        = entity.item_id,
            AddedAt       = entity.added_at,
            Title         = (c.BrandName != null ? c.BrandName + " " : "") + c.model,
            Subtitle      = $"{c.transmission} · {c.seats_count} seats",
            ImageUrl      = c.ImageUrl,
            Price         = c.Price,
            Rating        = c.Rating,
            BadgeText     = c.CategoryName
        };
    }

    private static FavoriteDto FallbackDto(favorite entity, FavoriteCategory category)
        => new()
        {
            FavoriteId    = entity.id,
            UserId        = entity.user_id,
            Category      = category,
            CategoryLabel = category.ToString(),
            ItemId        = entity.item_id,
            AddedAt       = entity.added_at,
            Title         = "[Item no longer available]"
        };
}
