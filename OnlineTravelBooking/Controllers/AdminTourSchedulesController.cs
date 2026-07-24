using Application.Common.RateLimiting;
using Application.Features.TourSchedules.Commands.AdminCancelTourSchedule;
using Application.Features.TourSchedules.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineTravelBooking.Controllers;

[Route("api/admin/tour-schedules")]
[ApiController]
[Authorize(Roles = "Admin")]
public sealed class AdminTourSchedulesController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminTourSchedulesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Add a new schedule to an existing tour. Validates against duplicates.
    /// </summary>
    [HttpPost("/api/admin/tours/{tourId:long}/schedules")]
    [EnableRateLimiting(RateLimitingPolicies.TourWrite)]
    public async Task<IActionResult> Create(long tourId, [FromBody] CreateTourScheduleRequest request)
    {
        var result = await _mediator.Send(new Application.Features.TourSchedules.Commands.CreateTourSchedule.CreateTourScheduleCommand(
            tourId,
            request.PriceTierId,
            request.StartDate,
            request.EndDate,
            request.Capacity
        ));

        return CreatedAtAction(nameof(Cancel), new { scheduleId = result.Data }, result);
    }

    /// <summary>
    /// Cancel a tour schedule and cascade cancellation to all active bookings.
    /// </summary>
    [HttpPost("{scheduleId:long}/cancel")]
    [EnableRateLimiting(RateLimitingPolicies.TourWrite)]
    public async Task<IActionResult> Cancel(long scheduleId, [FromBody] AdminCancelTourScheduleRequest request)
    {
        var result = await _mediator.Send(new AdminCancelTourScheduleCommand(scheduleId, request.Reason));
        return Ok(result);
    }
}
