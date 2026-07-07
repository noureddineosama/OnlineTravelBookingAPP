using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Pagination;
using Application.Features.Passengers.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Features.Passengers.Queries.GetAllPassengers;

/// <summary>
/// Inherits Page and PageSize from PagedQuery.
/// Add any filter/sort fields here (e.g. Status, SearchTerm) as init properties.
/// </summary>
public sealed record GetAllPassengersQuery : PagedQuery, IRequest<ApiResponse<PagedResult<PassengerResponse>>>
{
    public string? Status { get; init; }
    public string? SearchTerm { get; init; }
}

public sealed class GetAllPassengersQueryHandler
    : IRequestHandler<GetAllPassengersQuery, ApiResponse<PagedResult<PassengerResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllPassengersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper  = mapper;
    }

    public async Task<ApiResponse<PagedResult<PassengerResponse>>> Handle(
        GetAllPassengersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.passengers.AsQueryable();

        // ── Optional filters ─────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(p => p.status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(p =>
                p.name.Contains(request.SearchTerm) ||
                p.email.Contains(request.SearchTerm));

        // ── Sort + paginate using extension method ────────────
        var paged = await query
            .OrderBy(p => p.id)
            .ToPagedResultAsync(request, cancellationToken);

        // ── Project entities → DTOs while keeping metadata ───
        return ApiResponse<PagedResult<PassengerResponse>>.Ok(
            paged.MapTo(items => (IReadOnlyList<PassengerResponse>)
                _mapper.Map<List<PassengerResponse>>(items)));
    }
}
