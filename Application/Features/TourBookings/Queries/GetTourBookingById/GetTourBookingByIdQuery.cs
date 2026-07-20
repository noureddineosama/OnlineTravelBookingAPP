using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.TourBookings.DTOs;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourBookings.Queries.GetTourBookingById;

public sealed record GetTourBookingByIdQuery(long BookingId)
    : IRequest<ApiResponse<TourBookingResponse>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class GetTourBookingByIdQueryValidator : AbstractValidator<GetTourBookingByIdQuery>
{
    public GetTourBookingByIdQueryValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId must be a valid ID.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class GetTourBookingByIdQueryHandler
    : IRequestHandler<GetTourBookingByIdQuery, ApiResponse<TourBookingResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetTourBookingByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper  = mapper;
    }

    public async Task<ApiResponse<TourBookingResponse>> Handle(
        GetTourBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _uow.Repository<Domain.Entities.booking>().Query()
            .Where(b => b.id == request.BookingId && b.category == "tour")
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.tour)
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.price_tier)
            .FirstOrDefaultAsync(cancellationToken);

        if (booking is null)
            throw new NotFoundException("Tour booking", request.BookingId);

        if ((booking.status == Domain.Enums.BookingStatus.Cancelled.ToString() || booking.IsCancelled == true) && 
            booking.cancellation_reason_type != Domain.Enums.CancellationReasonType.AdminCancelled)
            throw new NotFoundException("Tour booking", request.BookingId);

        return ApiResponse<TourBookingResponse>.Ok(_mapper.Map<TourBookingResponse>(booking));
    }
}
