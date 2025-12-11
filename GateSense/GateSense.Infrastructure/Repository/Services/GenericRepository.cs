using System.Linq.Expressions;
using Domain.Models;
using Infrastructure.Common.Errors.Common.Repository;
using Infrastructure.Common.PagedList;
using Infrastructure.Common.Predicate;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Data;
using Infrastructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.Services;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    public readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    protected DbSet<T> Table => _context.Set<T>();

    public virtual async Task<Result<IEnumerable<T>>> GetListByConditionAsync(
        Expression<Func<T, bool>> condition,
        IEnumerable<Func<IQueryable<T>, 
        IQueryable<T>>>? includes = null
        )
    {
        try
        {
            IQueryable<T> query = Table.AsNoTracking().AsQueryable();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }
            
            var result = await query.Where(condition).ToListAsync();
            return Result<IEnumerable<T>>.Success(result);
        }
        catch (DbUpdateException ex)
        {
            return Result<IEnumerable<T>>.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public virtual async Task<Result<T>> GetSingleByConditionAsync(
        Expression<Func<T, bool>> condition,
        IEnumerable<Func<IQueryable<T>, 
        IQueryable<T>>>? includes = null)
    {
        try
        {
            IQueryable<T> query = Table.AsQueryable();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            var result = await query.FirstOrDefaultAsync(condition);

            if (result == null)
            {
                return Result<T>.Failure(RepositoryErrors<T>.NotFoundError);
            }

            return Result<T>.Success(result);
        }
        catch (DbUpdateException ex)
        {
            return Result<T>.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public virtual async Task<Result<int>> AddAsync(T item)
    {
        try
        {
            Table.Add(item);
            await SaveAsync();
            return Result<int>.Success(item.Id);
        }
        catch (DbUpdateException ex)
        {
            return Result<int>.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public virtual async Task<Result> AddRangeAsync(IEnumerable<T> items)
    {
        try
        {
            Table.AddRange(items);
            await SaveAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(RepositoryErrors<T>.AddError);
        }
    }

    public virtual async Task<Result> UpdateAsync(T item)
    {
        try
        {
            Table.Update(item);
            await SaveAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public async Task<Result> DeleteAsync(Expression<Func<T, bool>> condition)
    {
        try
        {
            var deletedCount = await Table.Where(condition).ExecuteDeleteAsync();

            if (deletedCount == 0)
            {
                return Result.Failure(RepositoryErrors<T>.NotFoundError);
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public virtual async Task<Result<IPagedList<T>>> FetchPaginatedByConditions(
        IEnumerable<(Expression<Func<T, bool>> predicate, PredicateOptions options)> conditions,
        (Expression<Func<T, object>> expression, bool isDesc) orderBy,
        IEnumerable<Func<IQueryable<T>, IQueryable<T>>> includes,
        bool isNoTracking,
        bool isSplitQuery,
        int pageNumber,
        int pageSize)
    {
        try
        {
            PredicateBuilder<T> predicate = new PredicateBuilder<T>();
            foreach (var expression in conditions)
            {
                switch (expression.options)
                {
                    case PredicateOptions.OR:
                        predicate.Or(expression.predicate);
                        break;
                    case PredicateOptions.AND:
                        predicate.And(expression.predicate);
                        break;
                    case PredicateOptions.NOT:
                        predicate.Not(expression.predicate);
                        break;
                }
            }

            IQueryable<T> query = Table;
            query = isNoTracking ? query.AsNoTracking() : query;
            query = isSplitQuery ? query.AsSplitQuery() : query;
            query = query.Where(predicate.Build());

            foreach (var include in includes)
            {
                query = include(query);
            }

            IOrderedQueryable<T> orderedQuery = orderBy.isDesc
                ? query.OrderByDescending(orderBy.expression)
                : query.OrderBy(orderBy.expression);

            var result = await PagedList<T>.CreateAsync(orderedQuery, pageNumber, pageSize);
            return Result<IPagedList<T>>.Success(result);
        }
        catch (DbUpdateException ex)
        {
            return Result<IPagedList<T>>.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }


    public async Task SaveAsync()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedOn = now;
            }

            entity.LastModifiedOn = now;
        }

        await _context.SaveChangesAsync();
    }
}