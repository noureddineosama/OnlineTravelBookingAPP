using Application.Features.Favorites.Commands.AddFavorite;
using Application.Features.Favorites.Commands.RemoveFavorite;
using Application.Features.Favorites.Queries.CheckFavorite;
using Application.Features.Favorites.Queries.GetMyFavorites;
using Application.Features.Favorites.Requests;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/favorites")]
[ApiController]
public sealed class FavoritesController : ControllerBase
{
    private readonly ISender _mediator;

    public FavoritesController(ISender mediator) => _mediator = mediator;

    /// <summary>Add any item (Tour, Hotel, Flight, Car) to a user's favourites.</summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddFavoriteRequest request)
    {
        var result = await _mediator.Send(
            new AddFavoriteCommand(request.UserId, request.Category, request.ItemId));

        return CreatedAtAction(nameof(GetMyFavorites), new { userId = request.UserId }, result);
    }

    /// <summary>Remove any item from a user's favourites.</summary>
    [HttpDelete]
    public async Task<IActionResult> Remove([FromBody] RemoveFavoriteRequest request)
    {
        var result = await _mediator.Send(
            new RemoveFavoriteCommand(request.UserId, request.Category, request.ItemId));

        return Ok(result);
    }

    /// <summary>
    /// Get all favourites for a user, paginated.
    /// Pass category=1 (Tour), 2 (Hotel), 3 (Flight), 4 (Car) to filter.
    /// Omit category to get all types.
    /// </summary>
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetMyFavorites(
        long                   userId,
        [FromQuery] FavoriteCategory? category = null,
        [FromQuery] int        page     = 1,
        [FromQuery] int        pageSize = 20)
    {
        var result = await _mediator.Send(new GetMyFavoritesQuery
        {
            UserId   = userId,
            Category = category,
            Page     = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    /// <summary>
    /// Check if a specific item is in a user's favourites.
    /// Returns IsFavorited (bool) and FavoriteId (for instant removal).
    /// </summary>
    [HttpGet("{userId:long}/check")]
    public async Task<IActionResult> Check(
        long                   userId,
        [FromQuery] FavoriteCategory category,
        [FromQuery] long        itemId)
    {
        var result = await _mediator.Send(
            new CheckFavoriteQuery(userId, category, itemId));

        return Ok(result);
    }
}
