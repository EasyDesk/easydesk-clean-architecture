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
}
