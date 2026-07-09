using Application.Common.Patterns;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations.
/// This allows feature handlers to avoid direct coupling to EF Core DbContext.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);

    Task<PaginatedResult<TResult>> GetPaginationAsync<TResult>(
                                       Expression<Func<T, bool>> predicate,
                                       Expression<Func<T, TResult>> selector,
                                       int page,
                                       int pageSize,
                                       string message = null,
                                       CancellationToken cancellationToken = default,
                                       params Expression<Func<T, object>>[] includes);

    Task<TResult> GetSelectorAsync<TResult>(Expression<Func<T, bool>> predicate,
                                     Expression<Func<T, TResult>> selector,
                                     CancellationToken cancellationToken = default,
                                    params Expression<Func<T, object?>>[]? includes);
    Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate 
                        , CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, 
                         CancellationToken cancellationToken);
}
