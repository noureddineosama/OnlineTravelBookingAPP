using Application.Features.FlightBookings.Commands.CreateFlightBooking;
using Application.Features.FlightBookings.DTOs;
using Application.Features.FlightBookings.Queries.GetFlightBookingById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class FlightBookingsController : ControllerBase
{
    private readonly ISender _mediator;

    public FlightBookingsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a flight booking by ID.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetFlightBookingByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new flight booking.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateFlightBookingRequest request)
    {
        var result = await _mediator.Send(new CreateFlightBookingCommand(
            request.UserId,
            request.FlightId,
            request.ReturnFlightId,
            request.TripType,
            request.Passengers));

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result);
    }
}

/// <summary>
/// Body for POST /api/flightbookings
/// </summary>
public sealed record CreateFlightBookingRequest(
    long UserId,
    long FlightId,
    long? ReturnFlightId,
    string TripType,
    IReadOnlyList<FlightBookingPassengerRequest> Passengers);