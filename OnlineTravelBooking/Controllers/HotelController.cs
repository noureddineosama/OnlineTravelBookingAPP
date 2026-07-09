using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelAvailability.Queries;
using Application.Features.Hotels.DTOs;
using Application.Features.Hotels.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers
{
    [ApiController]
    [Route("api/hotel")]
    [Authorize]
    public class HotelController : ControllerBase
    {
        private readonly IMediator mediator;

        public HotelController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(PaginatedResult<SearchHotelResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResult<SearchHotelResponseDTO>>> SearchHotelAsync([FromQuery] SearchRequestDTO requestDTO, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new SearchQuery(requestDTO), cancellationToken);
            if (result == null)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("details/{id}")]
        [ProducesResponseType(typeof(GenericResult<HotelDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<HotelDetailsResponseDTO>>> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            var calling = await mediator.Send(new HotelDetailsQuery(id), cancellationToken);
            if (calling == null)
                return BadRequest(calling);
            return Ok(calling);
        }

        [HttpGet("hotel-availability/check")]
        [ProducesResponseType(typeof(GenericResult<CheckRoomAvailabilityResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<CheckRoomAvailabilityResponseDTO>>> CheckAvailabilityAsync
                                                    ([FromQuery] CheckRoomAvailabilityRequestDTO requestDTO, CancellationToken cancellationToken)
        {
            var calling = await mediator.Send(new CheckRoomQuery(requestDTO), cancellationToken);
            if (calling == null)
                return BadRequest(calling);
            return Ok(calling);
        }
    }
}
