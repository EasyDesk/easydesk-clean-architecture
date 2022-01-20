using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public static class QueryableUtils
{
    public static async Task<Option<T>> FirstOptionAsync<T>(this IQueryable<T> query)
        where T : class
    {
        return (await query.FirstOrDefaultAsync()).AsOption();
    }

    public static IQueryable<T> Wrap<T>(this IQueryable<T> query, QueryWrapper<T> op)
    {
        return query.Conditionally(op != null, op);
    }

    public static IQueryable<T> Conditionally<T>(this IQueryable<T> query, bool condition, QueryWrapper<T> op)
    {
        return condition ? op(query) : query;
    }

    public static IQueryable<T> Conditionally<T, F>(this IQueryable<T> query, Option<F> filter, Func<F, QueryWrapper<T>> op)
    {
        return filter.Match(
            some: f => query.Wrap(op(f)),
            none: () => query);
    }

    public static Task<T> MaxByAsync<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector)
    {
        return query.OrderByDescending(keySelector).FirstOrDefaultAsync();
    }

    public static Task<T> MinByAsync<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector)
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

    public static async Task<Page<T>> GetPage<T>(this IQueryable<T> query, Pagination pagination)
    {
        var rowCount = await query.CountAsync();

        IEnumerable<T> values = await query
            .Skip(pagination.PageIndex * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToArrayAsync();

        return new Page<T>(values, pagination, rowCount);
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
