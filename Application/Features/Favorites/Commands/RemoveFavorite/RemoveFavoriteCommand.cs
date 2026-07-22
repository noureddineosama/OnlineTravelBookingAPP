using Application.Common.Caching;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Favorites.Cache;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Favorites.Commands.RemoveFavorite;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record RemoveFavoriteCommand(
    long             UserId,
    FavoriteCategory Category,
    long             ItemId
) : IRequest<Unit>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class RemoveFavoriteCommandValidator : AbstractValidator<RemoveFavoriteCommand>
{
    public RemoveFavoriteCommandValidator()
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

public sealed class RemoveFavoriteCommandHandler
    : IRequestHandler<RemoveFavoriteCommand, Unit>
{
    private readonly IUnitOfWork   _uow;
    private readonly ICacheService _cache;

    public RemoveFavoriteCommandHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow   = uow;
        _cache = cache;
    }

    public async Task<Unit> Handle(
        RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var categoryStr = request.Category.ToDbString();

        // Lookup by composite business key (same columns as the UQ index)
        var entity = await _uow.Repository<favorite>()
            .GetByIdAsync(f =>
                f.user_id  == request.UserId &&
                f.category == categoryStr    &&
                f.item_id  == request.ItemId,
                cancellationToken);

        if (entity is null)
            throw new NotFoundException("Favourite",
                $"UserId={request.UserId}, Category={request.Category}, ItemId={request.ItemId}");

        _uow.Repository<favorite>().Remove(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        // Invalidate user's list (all pages) and the per-item check key.
        await _cache.RemoveByPrefixAsync(FavoriteCacheKeys.UserPrefix(request.UserId), cancellationToken);
        await _cache.RemoveAsync(
            FavoriteCacheKeys.Check(request.UserId, categoryStr, request.ItemId),
            cancellationToken);

        return Unit.Value;
    }
}
