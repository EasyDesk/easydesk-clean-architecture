using EasyDesk.Tools.Collections;

namespace EasyDesk.CleanArchitecture.Application.Pagination;

public abstract class Pageable<T>
{
    public abstract Task<int> GetTotalCount();

    public abstract Task<IEnumerable<T>> GetPage(int pageSize, int pageIndex);

    public abstract IAsyncEnumerable<IEnumerable<T>> GetAllPages(int pageSize);
}

public static class Pageable
{
    public static Pageable<T> ToPageable<T>(this IEnumerable<T> sequence) => new EnumerablePageable<T>(sequence);

    private class EnumerablePageable<T> : Pageable<T>
    {
        private readonly IEnumerable<T> _items;

        public EnumerablePageable(IEnumerable<T> items)
        {
            _items = items;
        }

        public override Task<int> GetTotalCount() => Task.FromResult(_items.Count());

        public override Task<IEnumerable<T>> GetPage(int pageSize, int pageIndex)
        {
            return Task.FromResult(_items
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList()
                .AsEnumerable());
        }

        public override IAsyncEnumerable<IEnumerable<T>> GetAllPages(int pageSize)
        {
            return _items
                .Chunk(pageSize)
                .Cast<IEnumerable<T>>()
                .ToAsyncEnumerable();
        }
    }

    public static async IAsyncEnumerable<T> GetAllItems<T>(this Pageable<T> pageable, int pageSize)
    {
        await foreach (var page in pageable.GetAllPages(pageSize))
        {
            foreach (var item in page)
            {
                yield return item;
            }
        }
    }

    public static async Task<PageInfo<T>> GetPageInfo<T>(this Pageable<T> pageable, int pageSize, int pageIndex)
    {
        var page = await pageable.GetPage(pageSize, pageIndex);
        var totalCount = await pageable.GetTotalCount();
        return new(page, totalCount, pageSize, pageIndex);
    }
}
