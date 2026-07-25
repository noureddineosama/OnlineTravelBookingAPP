using Application.Common.Patterns;
using Application.Features.HotelBooking.Commands;
using Application.Features.HotelBooking.DTOs;
using Application.Features.HotelBooking.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineTravelBooking.Controllers
{
    [ApiController]
    [Route("api/hotel-bookings")]
    [Authorize]
    [EnableRateLimiting("auth-fixed-window")] 
    public class HotelBookingController : ControllerBase
    {
        private readonly IMediator mediator;

        public HotelBookingController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GenericResult<CreateHotelBookingResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<CreateHotelBookingResponseDTO>>> CreateBooking([FromBody]CreateHotelBookingRequestDTO requestDTO,
                                                                                                     CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateHotelBookingCommand(requestDTO), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GenericResult<UpdateHotelBookingResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<UpdateHotelBookingResponseDTO>>> UpdateBooking(long id,UpdateHotelBookingRequestDTO requestDTO,
                                                                                                  CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateHotelBookingCommand(id, requestDTO), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(GenericResult<CancelHotelBookingResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<CancelHotelBookingResponseDTO>>> ChangeStatusToBeCancelledAsync(long id,
                                                                                                  CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CancelHotelBookingQuery(id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GenericResult<HotelBookingDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<HotelBookingDetailsResponseDTO>>> DetailsByIdAsync(long id
                                                                                 , CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new HotelBookingDetailsQuery(id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("my-booking")]
        [ProducesResponseType(typeof(GenericResult<List<MyHotelBookingsResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<List<MyHotelBookingsResponseDTO>>>> MyBookingsDetailsASync(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new MyHotelBookingQuery(),cancellationToken);
            if (result == null)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
