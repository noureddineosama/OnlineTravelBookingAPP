using Application.Features.Flights.Queries.GetAllFlights;
using Application.Features.Flights.Queries.GetFlightById;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("flight-read")]
public sealed class FlightsController : ControllerBase
{
    private readonly ISender _mediator;

    public FlightsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search scheduled flights.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? origin = null,
        [FromQuery] string? destination = null,
        [FromQuery] DateTime? departureDateUtc = null,
        [FromQuery] string? cabinClass = null,
        [FromQuery] int? passengers = null)
    {
        var result = await _mediator.Send(new GetAllFlightsQuery
        {
            Page = page,
            PageSize = pageSize,
            OriginAirportCode = origin,
            DestinationAirportCode = destination,
            DepartureDateUtc = departureDateUtc,
            CabinClass = cabinClass,
            PassengersCount = passengers
        });

        return Ok(result);
    }

    /// <summary>
    /// Get a flight by ID.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetFlightByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }
}
