namespace Application.Common.Pagination;

/// <summary>
/// Generic paginated result wrapper returned by every list query.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Projects each item to a different type while preserving all pagination metadata.
    /// Example: pagedEntities.MapTo(items => mapper.Map&lt;List&lt;DTO&gt;&gt;(items))
    /// </summary>
    public PagedResult<TOut> MapTo<TOut>(Func<IReadOnlyList<T>, IReadOnlyList<TOut>> mapper)
        => new()
        {
            Items      = mapper(Items),
            TotalCount = TotalCount,
            Page       = Page,
            PageSize   = PageSize
        };
}
