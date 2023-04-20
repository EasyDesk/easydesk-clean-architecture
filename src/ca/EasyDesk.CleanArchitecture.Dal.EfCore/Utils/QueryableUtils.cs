using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.Commons.Collections;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public static class QueryableUtils
{
    public static async Task<Option<T>> FirstOptionAsync<T>(this IQueryable<T> query)
        where T : class
    {
        return (await query.FirstOrDefaultAsync()).AsOption();
    }

    public static IQueryable<T> Wrap<T>(this IQueryable<T> query, QueryWrapper<T>? op)
    {
        return query.Conditionally(op != null, op);
    }

    public static IQueryable<T> Conditionally<T>(this IQueryable<T> query, bool condition, QueryWrapper<T>? op)
    {
        return condition ? op?.Invoke(query) ?? query : query;
    }

    public static IQueryable<T> Conditionally<T, F>(this IQueryable<T> query, Option<F> filter, Func<F, QueryWrapper<T>> op) where F : notnull
    {
        return filter.Match(
            some: f => query.Wrap(op(f)),
            none: () => query);
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
            _ => query.OrderBy(keySelector)
        };
    }

    public static IOrderedQueryable<T> ThenBy<T, TKey>(this IOrderedQueryable<T> query, Expression<Func<T, TKey>> keySelector, OrderingDirection direction)
    {
        return direction switch
        {
            OrderingDirection.Descending => query.ThenByDescending(keySelector),
            _ => query.ThenBy(keySelector)
        };
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

        public async Task<int> GetTotalCount() => await _queryable.CountAsync();

        public async Task<IEnumerable<T>> GetPage(int pageSize, int pageIndex) =>
            await GetPageAsArray(pageSize, pageIndex);

        public async IAsyncEnumerable<IEnumerable<T>> GetAllPages(int pageSize)
        {
            var index = 0;
            T[] page;
            do
            {
                page = await GetPageAsArray(pageSize, index);
                if (page.Length > 0)
                {
                    yield return page;
                }
            }
            while (page.Length == pageSize);
        }

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

    public static async Task<IImmutableSet<T>> ToEquatableSetAsync<T>(this IQueryable<T> query) =>
        await query.ToListThenMap(x => x.ToEquatableSet());

    public static async Task<IImmutableSet<T>> ToEquatableSetAsync<T>(this IQueryable<T> query, IEqualityComparer<T> equalityComparer) =>
        await query.ToListThenMap(x => x.ToEquatableSet(equalityComparer));

    public static async Task<IImmutableList<T>> ToEquatableListAsync<T>(this IQueryable<T> query) =>
        await query.ToListThenMap(x => x.ToEquatableList());

    private static async Task<R> ToListThenMap<T, R>(this IQueryable<T> query, Func<IEnumerable<T>, R> mapper) =>
        await query.ToListAsync().Map(mapper);
}
