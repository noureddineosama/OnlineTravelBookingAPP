namespace Application.Common.Pagination;

/// <summary>
/// Base record for all paginated queries.
/// Inherit from this so every list query gets Page and PageSize for free.
/// </summary>
public abstract record PagedQuery
{
    /// <summary>1-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Number of items per page (max 100).</summary>
    public int PageSize { get; init; } = 20;
}
