using Application.Common.RateLimiting;
using Application.Features.Tours.Commands.CreateTour;
using Application.Features.Tours.Commands.DeleteTour;
using Application.Features.Tours.Commands.UpdateTour;
using Application.Features.Tours.Queries.GetAllTours;
using Application.Features.Tours.Queries.GetTourById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ToursController : ControllerBase
{
    private readonly ISender _mediator;

    public ToursController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of tours. Publicly accessible.
    /// </summary>
    [HttpGet]
    [EnableRateLimiting(RateLimitingPolicies.TourRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Domain.Enums.TourStatus? status = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetAllToursQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            Difficulty = difficulty,
            SearchTerm = search
        });

        return Ok(result);
    }

    /// <summary>
    /// Get details of a specific tour by ID. Publicly accessible.
    /// </summary>
    [HttpGet("{id:long}")]
    [EnableRateLimiting(RateLimitingPolicies.TourRead)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetTourByIdQuery(id));
        
        if (!result.Success)
            return StatusCode(result.StatusCode, result);
            
        return Ok(result);
    }

    /// <summary>
    /// Create a new tour. Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitingPolicies.TourWrite)]
    public async Task<IActionResult> Create([FromBody] CreateTourCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return StatusCode(result.StatusCode, result);
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.TourId }, result);
    }

    /// <summary>
    /// Update an existing tour. Admin only.
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitingPolicies.TourWrite)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateTourCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch.");

        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return StatusCode(result.StatusCode, result);
            
        return Ok(result);
    }

    /// <summary>
    /// Delete a tour. Admin only.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitingPolicies.TourWrite)]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteTourCommand(id));
        
        if (!result.Success)
            return StatusCode(result.StatusCode, result);
            
        return Ok(result);
    }
}
