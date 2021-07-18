using EasyDesk.CleanArchitecture.Application.Utils;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Utils
{
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
    }
}
