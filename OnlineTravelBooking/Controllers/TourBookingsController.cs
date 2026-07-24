using Application.Common.Interfaces;
using Application.Common.RateLimiting;
using Application.Features.TourBookings.Commands.CancelTourBooking;
using Application.Features.TourBookings.Commands.CreateTourBooking;
using Application.Features.TourBookings.Queries.GetTourBookingById;
using Application.Features.TourBookings.Queries.GetUserTourBookings;
using Application.Features.TourBookings.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineTravelBooking.Controllers;

[Route("api/tour-bookings")]
[ApiController]
[Authorize(Roles = "Passenger")]
public sealed class TourBookingsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentIUserService _currentUserService;

    public TourBookingsController(ISender mediator, ICurrentIUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>Create a new tour booking for a given schedule.</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitingPolicies.TourBooking)]
    public async Task<IActionResult> Create([FromBody] CreateTourBookingRequest request)
    {
        var result = await _mediator.Send(
            new CreateTourBookingCommand(
                _currentUserService.UserId,
                request.TourScheduleId,
                request.AdultsCount,
                request.ChildrenCount,
                request.InfantsCount));

        return CreatedAtAction(nameof(GetById), new { bookingId = result.Data!.BookingId }, result);
    }

    /// <summary>Update an existing tour booking (passenger counts only).</summary>
    [HttpPut("{bookingId:long}")]
    [EnableRateLimiting(RateLimitingPolicies.TourBooking)]
    public async Task<IActionResult> Update(long bookingId, [FromBody] UpdateTourBookingRequest request)
    {
        var result = await _mediator.Send(
            new Application.Features.TourBookings.Commands.UpdateTourBooking.UpdateTourBookingCommand(
                bookingId,
                _currentUserService.UserId,
                request.AdultsCount,
                request.ChildrenCount,
                request.InfantsCount));

        return Ok(result);
    }

    /// <summary>Cancel an existing tour booking. Restores available slots on the schedule.</summary>
    [HttpPut("{bookingId:long}/cancel")]
    [EnableRateLimiting(RateLimitingPolicies.TourBooking)]
    public async Task<IActionResult> Cancel(long bookingId, [FromBody] CancelTourBookingRequest request)
    {
        var result = await _mediator.Send(new CancelTourBookingCommand(bookingId, _currentUserService.UserId));
        return Ok(result);
    }

    /// <summary>Get a user's tour bookings with pagination and optional status filter.</summary>
    [HttpGet("my-bookings")]
    [EnableRateLimiting(RateLimitingPolicies.TourRead)]
    public async Task<IActionResult> GetUserBookings(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        [FromQuery] string? status   = null)
    {
        var result = await _mediator.Send(new GetUserTourBookingsQuery
        {
            UserId   = _currentUserService.UserId,
            Page     = page,
            PageSize = pageSize,
            Status   = status
        });
        return Ok(result);
    }

    /// <summary>Get a single tour booking by its ID with full details.</summary>
    [HttpGet("{bookingId:long}")]
    [EnableRateLimiting(RateLimitingPolicies.TourRead)]
    public async Task<IActionResult> GetById(long bookingId)
    {
        var result = await _mediator.Send(new GetTourBookingByIdQuery(bookingId));
        return Ok(result);
    }
}
