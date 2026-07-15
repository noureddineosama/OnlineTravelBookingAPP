using Application.Features.Passengers.Commands.CreatePassenger;
using Application.Features.Passengers.Commands.DeletePassenger;
using Application.Features.Passengers.Commands.UpdatePassenger;
using Application.Features.Passengers.Queries.GetAllPassengers;
using Application.Features.Passengers.Queries.GetPassengerById;
using Application.Features.Passengers.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class PassengersController : ControllerBase
{
    private readonly ISender _mediator;

    public PassengersController(ISender mediator) => _mediator = mediator;

    /// <summary>Get all passengers with pagination, optional status filter and free-text search.</summary>
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
        return Ok(result);
    }

    /// <summary>Create a new passenger.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePassengerRequest request)
    {
        var result = await _mediator.Send(
            new CreatePassengerCommand(request.Name, request.Email, request.Phone, request.RoleId));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>Update a passenger's name, phone, or status.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePassengerRequest request)
    {
        var result = await _mediator.Send(
            new UpdatePassengerCommand(id, request.Name, request.Phone, request.Status));
        return Ok(result);
    }

    /// <summary>Permanently delete a passenger.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeletePassengerCommand(id));
        return Ok(result);
    }
}
