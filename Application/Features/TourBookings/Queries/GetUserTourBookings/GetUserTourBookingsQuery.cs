using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Features.TourBookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourBookings.Queries.GetUserTourBookings;

/// <summary>
/// Paginated query to retrieve a user's tour bookings with full details.
/// </summary>
public sealed record GetUserTourBookingsQuery : PagedQuery, IRequest<ApiResponse<PagedResult<TourBookingResponse>>>
{
    public long    UserId { get; init; }
    public string? Status { get; init; }
}

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class GetUserTourBookingsQueryValidator : AbstractValidator<GetUserTourBookingsQuery>
{
    // Finite set of valid booking statuses defined by the domain
    private static readonly string[] ValidStatuses = ["confirmed", "cancelled", "completed", "pending"];

    public GetUserTourBookingsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
            .When(x => x.Status is not null);
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class GetUserTourBookingsQueryHandler
    : IRequestHandler<GetUserTourBookingsQuery, ApiResponse<PagedResult<TourBookingResponse>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetUserTourBookingsQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper  = mapper;
    }

    public async Task<ApiResponse<PagedResult<TourBookingResponse>>> Handle(
        GetUserTourBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<Domain.Entities.booking>().Query()
            .Where(b => b.user_id == request.UserId && b.category == "tour" && 
                        (!(b.status == BookingStatus.Cancelled.ToString() || b.IsCancelled == true) || 
                         b.cancellation_reason_type == CancellationReasonType.AdminCancelled))
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.tour)
            .Include(b => b.tour_booking)
                .ThenInclude(tb => tb.tour_schedule)
                    .ThenInclude(s => s.price_tier)
            .OrderByDescending(b => b.created_at)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<BookingStatus>(request.Status, ignoreCase: true, out var statusEnum))
            query = query.Where(b => b.status == statusEnum.ToString());

        var paged = await query.ToPagedResultAsync(request, cancellationToken);

        return ApiResponse<PagedResult<TourBookingResponse>>.Ok(
            paged.MapTo(items =>
                (IReadOnlyList<TourBookingResponse>)_mapper.Map<List<TourBookingResponse>>(items)));
    }
}
