using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.FavouriteTours.DTOs;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.FavouriteTours.Commands.AddFavouriteTour;

public sealed record AddFavouriteTourCommand(
    long UserId,
    long TourId
) : IRequest<ApiResponse<FavouriteTourResponse>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class AddFavouriteTourCommandValidator : AbstractValidator<AddFavouriteTourCommand>
{
    public AddFavouriteTourCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.TourId)
            .GreaterThan(0).WithMessage("TourId must be a valid ID.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class AddFavouriteTourCommandHandler
    : IRequestHandler<AddFavouriteTourCommand, ApiResponse<FavouriteTourResponse>>
{
    private readonly IApplicationDbContext _context;

    public AddFavouriteTourCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<FavouriteTourResponse>> Handle(
        AddFavouriteTourCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify passenger exists
        var passenger = await _context.passengers
            .FindAsync([request.UserId], cancellationToken);

        if (passenger is null)
            return ApiResponse<FavouriteTourResponse>.Fail(
                $"Passenger with ID '{request.UserId}' was not found.");

        // 2. Verify tour exists and is active
        var tour = await _context.tours
            .Include(t => t.location)
            .Include(t => t.tour_price_tiers)
            .FirstOrDefaultAsync(t => t.id == request.TourId, cancellationToken);

        if (tour is null)
            return ApiResponse<FavouriteTourResponse>.Fail(
                $"Tour with ID '{request.TourId}' was not found.");

        if (tour.status != "active")
            return ApiResponse<FavouriteTourResponse>.Fail(
                "Only active tours can be added to favourites.");

        // 3. Check for duplicate
        var alreadyFavourited = await _context.favorites
            .AnyAsync(f =>
                f.user_id == request.UserId &&
                f.category == "tour" &&
                f.item_id == request.TourId,
                cancellationToken);

        if (alreadyFavourited)
            return ApiResponse<FavouriteTourResponse>.Fail(
                "This tour is already in your favourites.");

        // 4. Insert new favourite
        var fav = new favorite
        {
            user_id  = request.UserId,
            category = "tour",
            item_id  = request.TourId,
            added_at = DateTime.UtcNow
        };

        await _context.favorites.AddAsync(fav, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Build response with tour details
        var lowestTier = tour.tour_price_tiers
            .OrderBy(t => t.adult_price)
            .FirstOrDefault();

        var response = new FavouriteTourResponse
        {
            FavouriteId      = fav.id,
            TourId           = tour.id,
            Title            = tour.title,
            Slug             = tour.slug,
            Summary          = tour.summary,
            MainImageUrl     = tour.main_image_url,
            DurationDays     = tour.duration_days,
            LocationCity     = tour.location?.city,
            LocationCountry  = tour.location?.country,
            StartingFromPrice = lowestTier?.adult_price,
            Currency         = lowestTier?.currency,
            AddedAt          = fav.added_at
        };

        return ApiResponse<FavouriteTourResponse>.Ok(
            response, "Tour added to favourites successfully.");
    }
}
