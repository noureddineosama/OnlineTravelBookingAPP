

using Application.Common.Interfaces;
using Application.Common.Patterns;
using MediatR.NotificationPublishers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace Infrastructure.Persistence;

/// <summary>
/// Concrete EF Core implementation of the generic IRepository interface.
/// </summary>
public sealed class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;

    public EfRepository(AppDbContext context) => _context = context;

    public async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
        => await _context.Set<T>().FindAsync([id], ct);

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => await _context.Set<T>().ToListAsync(ct);

    public IQueryable<T> Query()
        => _context.Set<T>();

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _context.Set<T>().AddAsync(entity, ct);

    //. not best practice 
    public async Task AddBulkDataAsync(List<T> entity, CancellationToken cancellationToken)
        => await _context.Set<T>().AddRangeAsync(entity);

    public async Task UpdateBulkDataAsync(List<T> entities, CancellationToken cancellationToken)
        => _context.Set<T>().UpdateRange(entities);


    public void Update(T entity)
        => _context.Set<T>().Update(entity);

    public void Remove(T entity)
        => _context.Set<T>().Remove(entity);

    public async Task<List<T>> GetListOfEntityAsync(Expression<Func<T, bool>> predicate,
                                       CancellationToken cancellationToken)
        => await _context.Set<T>().AsNoTracking().Where(predicate).ToListAsync();

    public async Task<PaginatedResult<TResult>> GetPaginationAsync<TResult>(
                                                    Expression<Func<T, bool>> predicate,
                                                    Expression<Func<T, TResult>> selector,
                                                    int page,
                                                    int pageSize,
                                                    string message = null,
                                                    CancellationToken cancellationToken = default,
                                                    params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null && includes.Length > 1)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (predicate != null)
            query = query.Where(predicate);

        var totalItems = await query.CountAsync();

        //.meaning that if the current page is 2 and page size is 5 that's meaning that i will skip the first 5 items and after that
        // . taking the second 5 items and like that 
        var items = await query.AsNoTracking().
                                Select(selector)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedResult<TResult>
        {
            IsSuccess = true,
            message = message,
            Data = items,
            pagination = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            }
        };
    }

    public async Task<List<TResult>> GetListSelectorAsync<TResult>(Expression<Func<T, bool>> predicate,
                                                     Expression<Func<T, TResult>> selector,
                                                     CancellationToken cancellationToken = default,
                                                     params Expression<Func<T, object?>>[]? includes)
    {
        var query = _context.Set<T>().AsQueryable();
        if (!includes.Any() || includes == null)
        {
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
        }

        query = query.Where(predicate);

        if (selector != null)
        {
            return await query.AsNoTracking().Select(selector).ToListAsync();
        }

        //. if the type of the TResult is the same as the type of T then we can return the entity directly without using the selector
        if (typeof(TResult) == typeof(T))
        {
            //. Read Method without tracking
            var entity = await query.AsNoTracking().ToListAsync();

            return (List<TResult>)(object)entity;
        }

        throw new InvalidOperationException(
                        "Selector is required when TResult is not TEntity"
                    );
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        //. Tracking is existing in this process 
        return await _context.Set<T>().AnyAsync(predicate);
    }

    public async Task<TResult> GetSelectorAsync<TResult>(Expression<Func<T, bool>> predicate, 
                                                  Expression<Func<T, TResult>>? selector,
                                                  CancellationToken cancellationToken = default,
                                                 params Expression<Func<T, object?>>[]? includes)
    {
        var query = _context.Set<T>().AsQueryable();
        if (!includes.Any() || includes == null)
        {
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
        }

        query = query.Where(predicate);

        if(selector != null)
        {
            return await query.AsNoTracking().Select(selector).FirstOrDefaultAsync();
        }

        //. if the type of the TResult is the same as the type of T then we can return the entity directly without using the selector
        if (typeof(TResult) == typeof(T))
        {
            //. Read Method without tracking
            var entity = await query.AsNoTracking().FirstOrDefaultAsync();

            return (TResult)(object)entity; 
        }

        throw new InvalidOperationException(
                        "Selector is required when TResult is not TEntity"
                    );
    }

    //. if you need to validate the entity before returning it,
    //           you can use this method to get the entity by its id and then validate it using the provided predicate
    public async Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate, 
                                      CancellationToken cancellationToken = default) =>
        await _context.Set<T>().AsQueryable().FirstOrDefaultAsync(predicate); //. tracked 
    public async Task<T> GetByIdAsync(
     Expression<Func<T, bool>> predicate,
     CancellationToken cancellationToken = default,
     params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        var query = _context.Set<T>().AsQueryable();

        return await query.Where(predicate).CountAsync();
    }
}
