using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Common.RateLimiting;
using Application.Features.Favorites.Commands.AddFavorite;
using Application.Features.Favorites.Commands.RemoveFavorite;
using Application.Features.Favorites.DTOs;
using Application.Features.Favorites.Queries.CheckFavorite;
using Application.Features.Favorites.Queries.GetMyFavorites;
using Application.Features.Favorites.Requests;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineTravelBooking.Controllers;

/// <summary>
/// Manages a user's favourites across all item categories (Tour, Hotel, Flight, Car).
/// </summary>
[Route("api/favorites")]
[ApiController]
[Authorize(Roles = "Passenger")]
public sealed class FavoritesController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentIUserService _currentUserService;

    public FavoritesController(ISender mediator, ICurrentIUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Add any item to a user's favourites.
    /// Returns a fully enriched favourite card (Title, Price, Rating, etc.)
    /// so the frontend can render the card immediately without a second request.
    /// </summary>
    /// <remarks>
    /// Category values: <c>Tour</c>, <c>Hotel</c>, <c>Flight</c>, <c>Car</c>
    /// </remarks>
    [HttpPost]
    [EnableRateLimiting(RateLimitingPolicies.FavoritesWrite)]
    [ProducesResponseType(typeof(ApiResponse<FavoriteDto>),          StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>),               StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>),               StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>),               StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] AddFavoriteRequest request)
    {
        var result = await _mediator.Send(
            new AddFavoriteCommand(_currentUserService.UserId, request.Category, request.ItemId));

        return CreatedAtAction(nameof(GetMyFavorites), null, result);
    }

    /// <summary>
    /// Remove a specific item from a user's favourites.
    /// Returns 204 No Content on success — no body.
    /// </summary>
    [HttpDelete]
    [EnableRateLimiting(RateLimitingPolicies.FavoritesWrite)]
    [ProducesResponseType(                                            StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>),               StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>),               StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(
        [FromQuery] FavoriteCategory category,
        [FromQuery] long             itemId)
    {
        await _mediator.Send(new RemoveFavoriteCommand(_currentUserService.UserId, category, itemId));
        return NoContent();
    }

    /// <summary>
    /// Get all favourites for a user, newest first, paginated.
    /// Optionally filter by category.
    /// </summary>
    /// <param name="category">Optional filter: Tour | Hotel | Flight | Car</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page, 1–100 (default: 20).</param>
    [HttpGet]
    [EnableRateLimiting(RateLimitingPolicies.FavoritesRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FavoriteDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),                   StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyFavorites(
        [FromQuery] FavoriteCategory? category = null,
        [FromQuery] int         page     = 1,
        [FromQuery] int         pageSize = 20)
    {
        var result = await _mediator.Send(new GetMyFavoritesQuery
        {
            UserId   = _currentUserService.UserId,
            Category = category,
            Page     = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    /// <summary>
    /// Check if a specific item is already in a user's favourites.
    /// Returns <c>isFavorited</c> (bool) and <c>favoriteId</c> so the
    /// frontend can call DELETE immediately without an extra lookup.
    /// </summary>
    [HttpGet("check")]
    [EnableRateLimiting(RateLimitingPolicies.FavoritesRead)]
    [ProducesResponseType(typeof(ApiResponse<CheckFavoriteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),           StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Check(
        [FromQuery] FavoriteCategory category,
        [FromQuery] long         itemId)
    {
        var result = await _mediator.Send(
            new CheckFavoriteQuery(_currentUserService.UserId, category, itemId));

        return Ok(result);
    }
}
