using Amazon.Util.Internal.PlatformServices;
using Application.Common.Patterns;
using Application.Features.Booking.Commands;
using Application.Features.Booking.DTOs;
using Application.Features.Booking.Quries;
using Application.Features.CarBookings.Commands;
using Application.Features.CarBookings.DTOs;
using Application.Features.CarBookings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineTravelBooking.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    [EnableRateLimiting("auth-fixed-window")]
    public class BookingController : ControllerBase 
    {
        private readonly IMediator mediator;

        public BookingController(IMediator  mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("my-bookings")]
        [ProducesResponseType(typeof(PaginatedResult<MyBookingsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResult<MyBookingsResponseDTO>>> MyBookingsAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new MyBookingsQuery(page, pageSize), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GenericResult<BookingDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<BookingDetailsResponseDTO>>> BookingDetailsAsync(long id , CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new BookingDetailsQuery(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id}/status")]
        [ProducesResponseType(typeof(GenericResult<GetBookingStatusResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<GetBookingStatusResponseDTO>>> GetBookingStatusAsync(long id ,CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetBookingStatusQuery(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(GenericResult<CancelBookingResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<CancelBookingResponseDTO>>> CancelBookingAsync(long id ,CancelBookingRequestDTO requestDTO ,CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CancelBookingCommand(id, requestDTO), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

    }
}
