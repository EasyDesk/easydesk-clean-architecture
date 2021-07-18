using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.Pages
{
    public record Page<T>(IEnumerable<T> Items, Pagination Pagination, int Count)
    {
        public int PageCount => (Count + Pagination.PageSize - 1) / Pagination.PageSize;
    }
}
