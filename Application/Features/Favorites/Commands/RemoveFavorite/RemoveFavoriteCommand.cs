using Application.Common.Exceptions;
using Application.Common.Interfaces;
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
    private readonly IApplicationDbContext _context;

    public RemoveFavoriteCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Unit> Handle(
        RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var categoryStr = request.Category.ToDbString();

        // Lookup by composite business key (same columns as the UQ index)
        var entity = await _context.favorites
            .FirstOrDefaultAsync(f =>
                f.user_id  == request.UserId &&
                f.category == categoryStr    &&
                f.item_id  == request.ItemId,
                cancellationToken);

        if (entity is null)
            throw new NotFoundException("Favourite",
                $"UserId={request.UserId}, Category={request.Category}, ItemId={request.ItemId}");

        _context.favorites.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
