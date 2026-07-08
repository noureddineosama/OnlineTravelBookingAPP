using Application.Features.FavouriteTours.Commands.AddFavouriteTour;
using Application.Features.FavouriteTours.Commands.RemoveFavouriteTour;
using Application.Features.FavouriteTours.Queries.CheckTourIsFavourite;
using Application.Features.FavouriteTours.Queries.GetUserFavouriteTours;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/favourite-tours")]
[ApiController]
public sealed class FavouriteToursController : ControllerBase
{
    private readonly ISender _mediator;

    public FavouriteToursController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Add a tour to a user's favourites.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddFavouriteTourRequest request)
    {
        var result = await _mediator.Send(
            new AddFavouriteTourCommand(request.UserId, request.TourId));

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetUserFavourites),
            new { userId = request.UserId }, result);
    }

    /// <summary>
    /// Remove a tour from a user's favourites.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Remove([FromBody] RemoveFavouriteTourRequest request)
    {
        var result = await _mediator.Send(
            new RemoveFavouriteTourCommand(request.UserId, request.TourId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a user's favourite tours with pagination.
    /// </summary>
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetUserFavourites(
        long userId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetUserFavouriteToursQuery
        {
            UserId   = userId,
            Page     = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    /// <summary>
    /// Check if a specific tour is in a user's favourites.
    /// </summary>
    [HttpGet("{userId:long}/check/{tourId:long}")]
    public async Task<IActionResult> Check(long userId, long tourId)
    {
        var result = await _mediator.Send(
            new CheckTourIsFavouriteQuery(userId, tourId));
        return Ok(result);
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

/// <summary>Body for POST /api/favourite-tours</summary>
public sealed record AddFavouriteTourRequest(long UserId, long TourId);

/// <summary>Body for DELETE /api/favourite-tours</summary>
public sealed record RemoveFavouriteTourRequest(long UserId, long TourId);
