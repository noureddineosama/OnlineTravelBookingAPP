using Microsoft.EntityFrameworkCore;

namespace Application.Common.Pagination;

/// <summary>
/// IQueryable extension methods for pagination.
/// Keeps query handlers clean — one call does the count + skip/take.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Executes a paginated query against the database.
    /// Runs COUNT and the page slice as two async DB calls.
    /// </summary>
    /// <param name="query">The IQueryable (must already have any Where/OrderBy applied).</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // Clamp to prevent abuse
        pageSize = Math.Clamp(pageSize, 1, 100);
        page     = Math.Max(page, 1);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items      = items.AsReadOnly(),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }

    /// <summary>
    /// Overload that accepts a <see cref="PagedQuery"/> base record directly.
    /// </summary>
    public static Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedQuery paged,
        CancellationToken ct = default)
        => query.ToPagedResultAsync(paged.Page, paged.PageSize, ct);
}
