using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Favorites.Cache;
using Application.Features.Favorites.DTOs;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Favorites.Queries.CheckFavorite;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record CheckFavoriteQuery(
    long             UserId,
    FavoriteCategory Category,
    long             ItemId
) : IRequest<ApiResponse<CheckFavoriteDto>>, ICacheableQuery
{
    // ── ICacheableQuery ──────────────────────────────────────────────────────
    /// <inheritdoc />
    /// User-scoped key: each user+category+item combination has its own cache slot.
    public string    CacheKey          => FavoriteCacheKeys.Check(UserId, Category.ToDbString(), ItemId);

    /// <inheritdoc />
    /// Returns <c>null</c> — driven by <c>CacheSettings.FavoritesCheckMinutes</c> in appsettings.json.
    public TimeSpan? SlidingExpiration => null;
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CheckFavoriteQueryValidator : AbstractValidator<CheckFavoriteQuery>
{
    public CheckFavoriteQueryValidator()
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

public sealed class CheckFavoriteQueryHandler
    : IRequestHandler<CheckFavoriteQuery, ApiResponse<CheckFavoriteDto>>
{
    private readonly IUnitOfWork _uow;

    public CheckFavoriteQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<ApiResponse<CheckFavoriteDto>> Handle(
        CheckFavoriteQuery request, CancellationToken cancellationToken)
    {
        var categoryStr = request.Category.ToDbString();

        // Project only the id — one lightweight SQL query, no entity tracking
        var favoriteId = await _uow.Repository<favorite>()
            .GetSelectorAsync(f =>
                f.user_id  == request.UserId &&
                f.category == categoryStr    &&
                f.item_id  == request.ItemId,
                f => (long?)f.id,
                cancellationToken);

        var dto = new CheckFavoriteDto
        {
            IsFavorited = favoriteId.HasValue,
            FavoriteId  = favoriteId
        };

        return ApiResponse<CheckFavoriteDto>.Ok(dto);
    }
}
