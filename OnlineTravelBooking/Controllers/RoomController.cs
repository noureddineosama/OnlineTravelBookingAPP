using Application.Common.Patterns;
using Application.Features.Hotels.DTOs;
using Application.Features.Rooms.Commands;
using Application.Features.Rooms.DTOs;
using Application.Features.Rooms.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Stripe.Tax;
using System.Runtime.CompilerServices;

namespace OnlineTravelBooking.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    [Authorize]
    [EnableRateLimiting("auth-fixed-window")]
    public class RoomController : ControllerBase
    {
        private readonly IMediator mediator;

        public RoomController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("hotels/{hotelId}/rooms")]
        [ProducesResponseType(typeof(GenericResult<CreateHotelResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<CreateHotelResponseDTO>>> AddNewRoomTOSpecificHotel(long hotelId,
                                                                                                         CreateRoomRequestDTO request,
                                                                                                         CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateRoomCommand(hotelId, request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{id}/extras")]
        [ProducesResponseType(typeof(GenericResult<CreateRoomExtraResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<CreateRoomExtraResponseDTO>>> AddExtrasRoomAsync(long id,
                                                                                                         CreateRoomExtraRequestDTO request,
                                                                                                         CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateRoomExtraCommand(id, request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GenericResult<UpdateRoomResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<RoomDetailsResponseDTO>>> UpdateRoomeDetailsAsync(long id,
                                                                                                       UpdateRoomRequestDTO requestDTO, 
                                                                                                       CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateRoomCommand(id, requestDTO), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}/update-price")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<string>>> UpdatePriceAsync(long id, UpdateRoomPriceRequestDTO requestDTO, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateRoomPriceCommand(id, requestDTO), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}/update-availability")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<string>>> UpdateAvailabilityAsync(long id, UpdateAvailabilityRequestDTO requestDTO, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateAvailabilityCommand(id, requestDTO), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}/update-extras")]
        [ProducesResponseType(typeof(GenericResult<UpdateRoomExtraResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<UpdateRoomExtraResponseDTO>>> UpdateRoomExtras(long id,UpdateRoomExtraRequestDTO requestDTO
                                                                                            , CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateRoomExtraCommand(id,requestDTO), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }



        [HttpGet("/hotels/{hotelid}/rooms/search")]
        [ProducesResponseType(typeof(PaginatedResult<GetHotelRoomsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResult<GetHotelRoomsResponseDTO>>> SearchRoom(long hotelid, int page,int pageSize
                                        ,[FromQuery] SearchRoomRequestDTO requestDTO,CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new SearchRoomQuery(hotelid, page, pageSize, requestDTO), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("hotels/{holtelId}/rooms")]
        [ProducesResponseType(typeof(GenericResult<HotelDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<HotelDetailsResponseDTO>>> GetHotelRoomsAsync(long hotelId, 
                                                                                                   int page = 1, int pageSize = 10,
                                                                                                   CancellationToken cancellationToken = default)
        {
            var calling = await mediator.Send(new GetHotelRoomsQuery(hotelId, page, pageSize), cancellationToken);
            if (calling == null)
                return BadRequest(calling);
            return Ok(calling);
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(GenericResult<RoomDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<RoomDetailsResponseDTO>>> RoomDetails(long id, 
                                                                                           CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new RoomDetailsQuery(id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }


        [HttpGet("{id}/availability")]
        [ProducesResponseType(typeof(GenericResult<GetAvailabilityResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<GetAvailabilityResponseDTO>>> GetAvailabilityAsync(long id,
                                                                                                        CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAvailabilityQuery(id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id}/extras")]
        [ProducesResponseType(typeof(GenericResult<RoomExtrasResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<RoomExtrasResponseDTO>>> GetRoomExtras(long id,
                                                                                                        CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetRoomExtrasQuery(id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(GenericResult<DeleteRoomResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<DeleteRoomResponseDTO>>> DeleteRoom(long id,
                                                                                              CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteRoomCommand(id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}/extras/{extraId}")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize (Roles = "Admin")]
        public async Task<ActionResult<GenericResult<string>>> RemoveRoomExtras(long id, long extraId,
                                                                                              CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new RemoveRoomExtraCommand(id,extraId), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }



        [HttpPatch("{id}/change-status")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<string>>> ChangeStatusAsync(long id,
                                                                                 ChangeRoomStatusRequestDTO requestDTO
                                                                                ,CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ChangeStatusCommand(id, requestDTO), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
