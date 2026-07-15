using Application.Features.CarBookings.Commands.CancelCarBooking;
using Application.Features.CarBookings.Commands.CreateCarBooking;
using Application.Features.CarBookings.Queries.GetCarBookingById;
using Application.Features.CarBookings.Queries.GetUserCarBookings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/car-bookings")]
[ApiController]
public sealed class CarBookingsController : ControllerBase
{
    private readonly ISender _mediator;

    public CarBookingsController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Create a new car booking.
    /// Calculates the rental price from the car's pricing tiers based on total hours.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCarBookingRequest request)
    {
        var result = await _mediator.Send(
            new CreateCarBookingCommand(
                request.UserId,
                request.CarId,
                request.PickupLocationId,
                request.DropoffLocationId,
                request.PickupAt,
                request.DropoffAt,
                request.DriverName,
                request.Extras.Select(e => new ExtraItem(e.ExtraId, e.Quantity)).ToList()));

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById),
            new { bookingId = result.Data!.BookingId }, result);
    }

    /// <summary>
    /// Cancel an existing car booking.
    /// </summary>
    [HttpPut("{bookingId:long}/cancel")]
    public async Task<IActionResult> Cancel(long bookingId, [FromBody] CancelCarBookingRequest request)
    {
        var result = await _mediator.Send(
            new CancelCarBookingCommand(bookingId, request.UserId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a user's car bookings with pagination and optional status filter.
    /// </summary>
    [HttpGet("user/{userId:long}")]
    public async Task<IActionResult> GetUserBookings(
        long userId,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        [FromQuery] string? status   = null)
    {
        var result = await _mediator.Send(new GetUserCarBookingsQuery
        {
            UserId   = userId,
            Page     = page,
            PageSize = pageSize,
            Status   = status
        });
        return Ok(result);
    }

    /// <summary>
    /// Get a single car booking by its booking ID with full details.
    /// </summary>
    [HttpGet("{bookingId:long}")]
    public async Task<IActionResult> GetById(long bookingId)
    {
        var result = await _mediator.Send(new GetCarBookingByIdQuery(bookingId));
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

/// <summary>Body for POST /api/car-bookings</summary>
public sealed record CreateCarBookingRequest(
    long              UserId,
    long              CarId,
    int               PickupLocationId,
    int               DropoffLocationId,
    DateTime          PickupAt,
    DateTime          DropoffAt,
    string?           DriverName,
    List<ExtraRequest> Extras);

/// <summary>An extra item included in a car booking request.</summary>
public sealed record ExtraRequest(int ExtraId, int Quantity);

/// <summary>Body for PUT /api/car-bookings/{bookingId}/cancel</summary>
public sealed record CancelCarBookingRequest(long UserId);
