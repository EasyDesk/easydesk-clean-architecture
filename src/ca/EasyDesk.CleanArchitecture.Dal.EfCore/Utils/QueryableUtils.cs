using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Comparers;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public static class QueryableUtils
{
    public static async Task<Option<T>> FirstOptionAsync<T>(this IQueryable<T> query)
    {
        var result = await query.Take(1).ToListAsync();
        return result.Count > 0 ? Some(result[0]) : None;
    }

    public static Task<T?> MaxByAsync<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector)
    {
        return query.OrderByDescending(keySelector).FirstOrDefaultAsync();
    }

    public static Task<T?> MinByAsync<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector)
    {
        return query.OrderBy(keySelector).FirstOrDefaultAsync();
    }

    public static IOrderedQueryable<T> OrderBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector, OrderingDirection direction)
    {
        return direction switch
        {
            OrderingDirection.Descending => query.OrderByDescending(keySelector),
            _ => query.OrderBy(keySelector),
        };
    }

    public static IOrderedQueryable<T> ThenBy<T, TKey>(this IOrderedQueryable<T> query, Expression<Func<T, TKey>> keySelector, OrderingDirection direction)
    {
        return direction switch
        {
            OrderingDirection.Descending => query.ThenByDescending(keySelector),
            _ => query.ThenBy(keySelector),
        };
    }

    public static IOrderedQueryable<T> OrderBy<T>(
        this IQueryable<T> query,
        (Expression<Func<T, object>> KeySelector, OrderingDirection Direction) first,
        params IEnumerable<(Expression<Func<T, object>> KeySelector, OrderingDirection Direction)> ordering)
    {
        return ordering.Aggregate(query.OrderBy(first.KeySelector, first.Direction), (q, pair) => q.ThenBy(pair.KeySelector, pair.Direction));
    }

    public static IOrderedQueryable<T> Order<T>(
        this IQueryable<T> query,
        Action<OrderingBuilder<T>> ordering)
    {
        var orderingBuilder = new OrderingBuilder<T>(query);
        ordering(orderingBuilder);
        return orderingBuilder.Build();
    }

    public static IPageable<T> ToPageable<T>(this IQueryable<T> queryable) =>
        new QueryablePageable<T>(queryable);

    private class QueryablePageable<T> : IPageable<T>
    {
        private readonly IQueryable<T> _queryable;

        public QueryablePageable(IQueryable<T> queryable)
        {
            _queryable = queryable;
        }

        public async Task<IEnumerable<T>> GetAll() => await _queryable.ToArrayAsync();

        public async Task<IEnumerable<T>> GetPage(int pageSize, int pageIndex) =>
            await GetPageAsArray(pageSize, pageIndex);

        private async Task<T[]> GetPageAsArray(int pageSize, int pageIndex)
        {
            IQueryable<T> SafeSkip()
            {
                var skip = pageIndex > int.MaxValue / pageSize
                    ? int.MaxValue
                    : pageIndex * pageSize;
                return _queryable.Skip(skip);
            }
            return await (pageIndex > 0 ? SafeSkip() : _queryable)
                .Take(pageSize)
                .ToArrayAsync();
        }
    }

    public static async Task<IFixedSet<T>> ToFixedSetAsync<T>(this IQueryable<T> query) =>
        await query.ToListThenMap(x => x.ToFixedSet());

    public static async Task<IFixedSet<T>> ToFixedSetAsync<T>(this IQueryable<T> query, IEqualityComparer<T> equalityComparer) =>
        await query.ToListThenMap(x => x.ToFixedHashSet(equalityComparer));

    public static async Task<IFixedList<T>> ToFixedListAsync<T>(this IQueryable<T> query) =>
        await query.ToListThenMap(x => x.ToFixedList());

    private static async Task<R> ToListThenMap<T, R>(this IQueryable<T> query, Func<IEnumerable<T>, R> mapper) =>
        await query.ToListAsync().Map(mapper);

    public static IQueryable<T> Wrap<T>(this IQueryable<T> query, QueryWrapper<T>? op)
    {
        return query.Conditionally(op is not null, x => op!(x));
    }
}

public class OrderingBuilder<T>
{
    private readonly IQueryable<T> _queryable;
    private IOrderedQueryable<T>? _orderedQueryable;

    internal OrderingBuilder(IQueryable<T> queryable)
    {
        _queryable = queryable;
    }

    public OrderingBuilder<T> By<K>(Expression<Func<T, K>> keySelector, OrderingDirection direction = OrderingDirection.Ascending)
    {
        _orderedQueryable = _orderedQueryable is null
            ? _queryable.OrderBy(keySelector, direction)
            : _orderedQueryable.ThenBy(keySelector, direction);
        return this;
    }

    public OrderingBuilder<T> ByDescending<K>(Expression<Func<T, K>> keySelector) =>
        By(keySelector, OrderingDirection.Descending);

    internal IOrderedQueryable<T> Build()
    {
        if (_orderedQueryable is null)
        {
            throw new InvalidOperationException("Ordering cannot be empty");
        }
        return _orderedQueryable;
    }
}
