using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.FavouriteTours.Queries.CheckTourIsFavourite;

public sealed record CheckTourIsFavouriteQuery(
    long UserId,
    long TourId
) : IRequest<ApiResponse<bool>>;

public sealed class CheckTourIsFavouriteQueryHandler
    : IRequestHandler<CheckTourIsFavouriteQuery, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _context;

    public CheckTourIsFavouriteQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(
        CheckTourIsFavouriteQuery request, CancellationToken cancellationToken)
    {
        var isFavourite = await _context.favorites
            .AnyAsync(f =>
                f.user_id == request.UserId &&
                f.category == "tour" &&
                f.item_id == request.TourId,
                cancellationToken);

        return ApiResponse<bool>.Ok(isFavourite);
    }
}
