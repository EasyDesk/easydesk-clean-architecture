﻿using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Application.Pagination;

public static class Pageable
{
    public static IPageable<T> FromEnumerable<T>(IEnumerable<T> sequence) => new EnumerablePageable<T>(sequence);

    private class EnumerablePageable<T> : IPageable<T>
    {
        private readonly IEnumerable<T> _items;

        public EnumerablePageable(IEnumerable<T> items)
        {
            _items = items;
        }

        public Task<int> GetTotalCount() => Task.FromResult(_items.Count());

        public Task<IEnumerable<T>> GetPage(int pageSize, int pageIndex)
        {
            IEnumerable<T> SafeSkip()
            {
                int skip;
                if (pageIndex > int.MaxValue / pageSize)
                {
                    skip = int.MaxValue;
                }
                else
                {
                    skip = pageIndex * pageSize;
                }
                return _items.Skip(skip);
            }
            return Task.FromResult(
                SafeSkip()
                .Take(pageSize)
                .ToList()
                .AsEnumerable());
        }

        public IAsyncEnumerable<IEnumerable<T>> GetAllPages(int pageSize)
        {
            return _items
                .Chunk(pageSize)
                .Cast<IEnumerable<T>>()
                .ToAsyncEnumerable();
        }
    }

    public static async IAsyncEnumerable<T> GetAllItems<T>(this IPageable<T> pageable, int pageSize)
    {
        await foreach (var page in pageable.GetAllPages(pageSize))
        {
            foreach (var item in page)
            {
                yield return item;
            }
        }
    }

    public static async Task<PageInfo<T>> GetPageInfo<T>(this IPageable<T> pageable, int pageSize, int pageIndex)
    {
        var page = await pageable.GetPage(pageSize, pageIndex);
        var totalCount = await pageable.GetTotalCount();
        return new(page, totalCount, pageSize, pageIndex);
    }
}