using Application.Common.Patterns;
using Application.Features.CarBookings.Commands;
using Application.Features.CarBookings.DTOs;
using Application.Features.CarBookings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineTravelBooking.Controllers
{
    [ApiController]
    [Route("api/car-bookings")]
    [Authorize]
    [EnableRateLimiting("auth-fixed-window")]
    public class CarBookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CarBookingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GenericResult<CarBookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<CarBookingResponse>>> CreateBooking(
            [FromBody] CreateCarBookingRequestDTO requestDTO,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateCarBookingCommand(requestDTO), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(GenericResult<CancelCarBookingResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<CancelCarBookingResponseDTO>>> CancelBooking(
            long id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelCarBookingCommand(id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GenericResult<CarBookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<CarBookingResponse>>> DetailsByIdAsync(
            long id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CarBookingDetailsQuery(id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("my-booking")]
        [ProducesResponseType(typeof(GenericResult<List<MyCarBookingsResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<List<MyCarBookingsResponseDTO>>>> MyBookingsDetailsAsync(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MyCarBookingsQuery(), cancellationToken);
            if (result == null)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
