using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Passengers.DTOs;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Application.Features.Passengers.Queries.GetPassengerById;

public sealed record GetPassengerByIdQuery(long Id)
    : IRequest<ApiResponse<PassengerResponse>>;

public sealed class GetPassengerByIdQueryValidator : AbstractValidator<GetPassengerByIdQuery>
{
    public GetPassengerByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Passenger ID must be greater than 0.");
    }
}

public sealed class GetPassengerByIdQueryHandler
    : IRequestHandler<GetPassengerByIdQuery, ApiResponse<PassengerResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPassengerByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper  = mapper;
    }

    public async Task<ApiResponse<PassengerResponse>> Handle(
        GetPassengerByIdQuery request, CancellationToken cancellationToken)
    {
        var passenger = await _context.passengers.FindAsync([request.Id], cancellationToken);

        if (passenger is null)
            throw new NotFoundException(nameof(passenger), request.Id);

        return ApiResponse<PassengerResponse>.Ok(_mapper.Map<PassengerResponse>(passenger));
    }
}

