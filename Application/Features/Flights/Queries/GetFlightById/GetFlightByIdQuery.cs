using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Flights.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Features.Flights.Queries.GetFlightById;

public sealed record GetFlightByIdQuery(long Id)
    : IRequest<ApiResponse<FlightResponse>>;

public sealed class GetFlightByIdQueryHandler
    : IRequestHandler<GetFlightByIdQuery, ApiResponse<FlightResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFlightByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<FlightResponse>> Handle(
        GetFlightByIdQuery request,
        CancellationToken cancellationToken)
    {
        var flight = await _context.flights
            .FindAsync([request.Id], cancellationToken);

        if (flight is null)
            throw new NotFoundException(nameof(flight), request.Id);

        return ApiResponse<FlightResponse>.Ok(
            _mapper.Map<FlightResponse>(flight));
    }
}