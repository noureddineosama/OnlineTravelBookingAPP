using Application.Features.FlightBookings.Commands.CreateFlightBooking;
using Application.Features.FlightBookings.Commands.DeleteFlightBooking;
using Application.Features.FlightBookings.Commands.UpdateFlightBooking;
using Application.Features.FlightBookings.DTOs;
using Application.Features.FlightBookings.Queries.GetFlightBookingById;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
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
    [EnableRateLimiting("flight-read")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetFlightBookingByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new flight booking.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("flight-write")]
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
    /// <summary>
    /// Update the itinerary and passenger list of an unpaid flight booking.
    /// </summary>
    [HttpPut("{id:long}")]
    [EnableRateLimiting("flight-write")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateFlightBookingRequest request)
    {
        var result = await _mediator.Send(new UpdateFlightBookingCommand(
            id,
            request.FlightId,
            request.ReturnFlightId,
            request.TripType,
            request.Passengers));

        return result.StatusCode switch
        {
            404 => NotFound(result),
            409 => Conflict(result),
            _ when !result.Success => BadRequest(result),
            _ => Ok(result)
        };
    }

    /// <summary>
    /// Permanently delete an unpaid flight booking and release its seats.
    /// </summary>
    [HttpDelete("{id:long}")]
    [EnableRateLimiting("flight-write")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteFlightBookingCommand(id));

        return result.StatusCode switch
        {
            404 => NotFound(result),
            409 => Conflict(result),
            _ when !result.Success => BadRequest(result),
            _ => Ok(result)
        };
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

/// <summary>
/// Body for PUT /api/flightbookings/{id}
/// </summary>
public sealed record UpdateFlightBookingRequest(
    long FlightId,
    long? ReturnFlightId,
    string TripType,
    IReadOnlyList<FlightBookingPassengerRequest> Passengers);
