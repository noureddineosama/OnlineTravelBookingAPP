using Application.Features.TourBookings.Commands.CancelTourBooking;
using Application.Features.TourBookings.Commands.CreateTourBooking;
using Application.Features.TourBookings.Queries.GetTourBookingById;
using Application.Features.TourBookings.Queries.GetUserTourBookings;
using Application.Features.TourBookings.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/tour-bookings")]
[ApiController]
public sealed class TourBookingsController : ControllerBase
{
    private readonly ISender _mediator;

    public TourBookingsController(ISender mediator) => _mediator = mediator;

    /// <summary>Create a new tour booking for a given schedule.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTourBookingRequest request)
    {
        var result = await _mediator.Send(
            new CreateTourBookingCommand(
                request.UserId,
                request.TourScheduleId,
                request.AdultsCount,
                request.ChildrenCount,
                request.InfantsCount));

        return CreatedAtAction(nameof(GetById), new { bookingId = result.Data!.BookingId }, result);
    }

    /// <summary>Cancel an existing tour booking. Restores available slots on the schedule.</summary>
    [HttpPut("{bookingId:long}/cancel")]
    public async Task<IActionResult> Cancel(long bookingId, [FromBody] CancelTourBookingRequest request)
    {
        var result = await _mediator.Send(new CancelTourBookingCommand(bookingId, request.UserId));
        return Ok(result);
    }

    /// <summary>Get a user's tour bookings with pagination and optional status filter.</summary>
    [HttpGet("user/{userId:long}")]
    public async Task<IActionResult> GetUserBookings(
        long userId,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        [FromQuery] string? status   = null)
    {
        var result = await _mediator.Send(new GetUserTourBookingsQuery
        {
            UserId   = userId,
            Page     = page,
            PageSize = pageSize,
            Status   = status
        });
        return Ok(result);
    }

    /// <summary>Get a single tour booking by its ID with full details.</summary>
    [HttpGet("{bookingId:long}")]
    public async Task<IActionResult> GetById(long bookingId)
    {
        var result = await _mediator.Send(new GetTourBookingByIdQuery(bookingId));
        return Ok(result);
    }
}
