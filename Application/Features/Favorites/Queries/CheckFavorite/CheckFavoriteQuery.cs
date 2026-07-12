using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Favorites.DTOs;
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
) : IRequest<ApiResponse<CheckFavoriteDto>>;

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
    private readonly IApplicationDbContext _context;

    public CheckFavoriteQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<ApiResponse<CheckFavoriteDto>> Handle(
        CheckFavoriteQuery request, CancellationToken cancellationToken)
    {
        var categoryStr = request.Category.ToString().ToLower();

        // Project only the id — one lightweight SQL query, no entity tracking
        var favoriteId = await _context.favorites
            .Where(f =>
                f.user_id  == request.UserId &&
                f.category == categoryStr    &&
                f.item_id  == request.ItemId)
            .Select(f => (long?)f.id)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new CheckFavoriteDto
        {
            IsFavorited = favoriteId.HasValue,
            FavoriteId  = favoriteId
        };

        return ApiResponse<CheckFavoriteDto>.Ok(dto);
    }
}
