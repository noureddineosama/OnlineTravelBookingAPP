using Application.Features.Passengers.Commands.CreatePassenger;
using Application.Features.Passengers.Commands.DeletePassenger;
using Application.Features.Passengers.Commands.UpdatePassenger;
using Application.Features.Passengers.Queries.GetAllPassengers;
using Application.Features.Passengers.Queries.GetPassengerById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class PassengersController : ControllerBase
{
    private readonly ISender _mediator;

    public PassengersController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Get all passengers with pagination, optional status filter and free-text search.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        [FromQuery] string? status   = null,
        [FromQuery] string? search   = null)
    {
        var result = await _mediator.Send(new GetAllPassengersQuery
        {
            Page       = page,
            PageSize   = pageSize,
            Status     = status,
            SearchTerm = search
        });
        return Ok(result);
    }

    /// <summary>Get a single passenger by ID.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetPassengerByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new passenger.
    /// Requires an existing role_id (use GET /api/roles to list available roles).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePassengerRequest request)
    {
        var result = await _mediator.Send(
            new CreatePassengerCommand(request.Name, request.Email, request.Phone, request.RoleId));

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById),
            new { id = result.Data!.Id }, result);
    }

    /// <summary>Update a passenger's name, phone, or status.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePassengerRequest request)
    {
        var result = await _mediator.Send(
            new UpdatePassengerCommand(id, request.Name, request.Phone, request.Status));
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Permanently delete a passenger.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeletePassengerCommand(id));
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

/// <summary>Body for POST /api/passengers</summary>
public sealed record CreatePassengerRequest(
    string  Name,
    string  Email,
    string? Phone  = null,
    int     RoleId = 1);

/// <summary>Body for PUT /api/passengers/{id}</summary>
public sealed record UpdatePassengerRequest(
    string? Name   = null,
    string? Phone  = null,
    string? Status = null);
