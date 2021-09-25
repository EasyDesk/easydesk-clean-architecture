using System;

namespace EasyDesk.CleanArchitecture.Application.Pages
{
    public record Pagination
    {
        public const int MaxPageSize = 100;

        public int PageIndex { get; }

        public int PageSize { get; }

        public Pagination(int pageIndex, int pageSize)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index cannot be negative");
            }
            if (pageSize is < 0 or > MaxPageSize)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size must be between 1 and {MaxPageSize}");
            }

            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}
