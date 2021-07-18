using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Pages
{
    public static class PageFactories
    {
        public static async Task<Page<T>> GetPage<T>(this IQueryable<T> query, Pagination pagination)
        {
            var rowCount = await query.CountAsync();

            IEnumerable<T> values = await query
                .Skip(pagination.PageIndex * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToArrayAsync();

            return new Page<T>(values, pagination, rowCount);
        }

        public static Page<T> GetPage<T>(this IEnumerable<T> sequence, Pagination pagination)
        {
            var materialized = sequence.ToList();
            var rowCount = materialized.Count;
            var values = materialized
                .Skip(pagination.PageIndex * pagination.PageSize)
                .Take(pagination.PageSize);

            return new Page<T>(values, pagination, rowCount);
        }
    }
}
