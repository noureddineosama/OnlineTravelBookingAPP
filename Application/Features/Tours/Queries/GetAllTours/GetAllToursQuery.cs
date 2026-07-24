using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Features.Tours.Cache;
using Application.Features.Tours.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tours.Queries.GetAllTours;

public sealed record GetAllToursQuery : PagedQuery, IRequest<ApiResponse<PagedResult<TourDto>>>, ICacheableQuery
{
    public TourStatus? Status     { get; init; }
    public string?     Difficulty { get; init; }
    public string?     SearchTerm { get; init; }

    // ── ICacheableQuery ──────────────────────────────────────────────────────
    /// <inheritdoc />
    public string CacheKey =>
        TourCacheKeys.List(Page, PageSize, Status?.ToString(), Difficulty, SearchTerm);

    /// <inheritdoc />
    /// Returns <c>null</c> — expiration is driven by <c>CacheSettings.TourListMinutes</c> in appsettings.json.
    public TimeSpan? SlidingExpiration => null;
}

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class GetAllToursQueryValidator : AbstractValidator<GetAllToursQuery>
{
    public GetAllToursQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

internal sealed class GetAllToursQueryHandler : IRequestHandler<GetAllToursQuery, ApiResponse<PagedResult<TourDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllToursQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<TourDto>>> Handle(GetAllToursQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<tour>().Query().AsNoTracking()
            .Include(t => t.tour_price_tiers)
            .Include(t => t.tour_schedules)
            .Where(t => !t.is_deleted);

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.status == request.Status.Value);
        }
        else
        {
            query = query.Where(t => t.status == TourStatus.Active); // Default to active if no status requested
        }

        if (!string.IsNullOrWhiteSpace(request.Difficulty))
        {
            query = query.Where(t => t.difficulty == request.Difficulty);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(t => t.title.Contains(request.SearchTerm) || t.summary.Contains(request.SearchTerm));
        }

        var paged = await query.ToPagedResultAsync(request, cancellationToken);

        return ApiResponse<PagedResult<TourDto>>.Ok(
            paged.MapTo(items =>
                (IReadOnlyList<TourDto>)_mapper.Map<List<TourDto>>(items)));
    }
}
