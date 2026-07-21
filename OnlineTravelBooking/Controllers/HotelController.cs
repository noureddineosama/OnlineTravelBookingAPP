using Application.Common.Patterns;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelAvailability.Queries;
using Application.Features.Hotels.Commands;
using Application.Features.Hotels.DTOs;
using Application.Features.Hotels.Queries;
using Application.Features.Images.DTOs;
using Application.Features.Images.HotelImages.Commands;
using Application.Features.Rooms.Commands;
using Application.Features.Rooms.DTOs;
using Application.Features.Rooms.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnlineTravelBooking.DTOs;
using System.Reflection.Metadata;

namespace OnlineTravelBooking.Controllers
{
    [ApiController]
    [Route("api/hotels")]
    [Authorize]
    [EnableRateLimiting("auth-fixed-window")]
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
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResult<SearchHotelResponseDTO>>> SearchHotelAsync([FromQuery] SearchRequestDTO requestDTO,
                                                                                                   CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new SearchQuery(requestDTO), cancellationToken);
            if (result == null)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("all-hotels")]
        [ProducesResponseType(typeof(PaginatedResult<SearchHotelResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResult<SearchHotelResponseDTO>>> GetAllHotelsAsyc([FromQuery] GetHotelsRequestDTO requestDTO,CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPagedHotelsQuery(requestDTO), cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.Data == null)
                    return NotFound(result.Data);
                return BadRequest(result);
            }
            return Ok(result);
        }

       
        [HttpPost]
        [ProducesResponseType(typeof(GenericResult<CreateHotelResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<CreateHotelResponseDTO>>> CreateHotelAsync(CreateHotelRequestDTO request, 
                                                                                                CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateHotelCommand(request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{id}/images")]
        [ProducesResponseType(typeof(GenericResult<IReadOnlyCollection<UploadImageResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<IReadOnlyCollection<UploadImageResponseDTO>>>> UploadImageAsync(long id,[FromForm]UplodImageRequestControllerDTO request, 
                                                                                                                    CancellationToken cancellationToken)
        {
            if (!request.images.Any())
                return BadRequest(request.images);
            var data_mapped = request.images.Select(image => new UploadImageRequestDTO
            {
                FileStream = image.OpenReadStream(),
                ContentType = image.ContentType,
                FileName = image.Name,
                FolderName = ImageFolder.Hotels.ToString()
            }).ToList();

            var result = await mediator.Send(new CreateHotelImageCommand(id,data_mapped), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GenericResult<UpdateHotelResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<UpdateHotelResponseDTO>>> UpdateHotelAsync(long id,UpdateHotelRequestDTO request, 
                                                                                               CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateHotelCommand(id, request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("details/{id}")]
        [ProducesResponseType(typeof(GenericResult<HotelDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<HotelDetailsResponseDTO>>> GetByIdAsync(long id,
                                                                                             CancellationToken cancellationToken)
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

        [HttpPatch("delete-hotel/{id}")]
        [ProducesResponseType(typeof(GenericResult<DeleteHotelResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<DeleteHotelResponseDTO>>> RemoveHotelAsync(long id, 
                                                                                  CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteHotelCommand(id), cancellationToken);
            if(result ==null )
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("change-status/{id}")]
        [ProducesResponseType(typeof(GenericResult<ChangeHotelStatusResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenericResult<ChangeHotelStatusResponseDTO>>> ChangeStatus(ChangeHotelStatusRequestDTO request, 
                                                                                  CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ChangeHotelStatusCommand(request), cancellationToken);
            if(result ==null )
                return BadRequest(result);
            return Ok(result);
        }
    }
}
