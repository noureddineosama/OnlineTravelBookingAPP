using Application.Common.Interfaces;
using Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.FavouriteTours.Commands.RemoveFavouriteTour;

public sealed record RemoveFavouriteTourCommand(
    long UserId,
    long TourId
) : IRequest<ApiResponse<string>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class RemoveFavouriteTourCommandValidator : AbstractValidator<RemoveFavouriteTourCommand>
{
    public RemoveFavouriteTourCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.TourId)
            .GreaterThan(0).WithMessage("TourId must be a valid ID.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class RemoveFavouriteTourCommandHandler
    : IRequestHandler<RemoveFavouriteTourCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;

    public RemoveFavouriteTourCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> Handle(
        RemoveFavouriteTourCommand request, CancellationToken cancellationToken)
    {
        var fav = await _context.favorites
            .FirstOrDefaultAsync(f =>
                f.user_id == request.UserId &&
                f.category == "tour" &&
                f.item_id == request.TourId,
                cancellationToken);

        if (fav is null)
            return ApiResponse<string>.Fail(
                "This tour is not in your favourites.");

        _context.favorites.Remove(fav);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok(
            "Removed.", "Tour removed from favourites successfully.");
    }
}
