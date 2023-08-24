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

        public Task<IEnumerable<T>> GetAll() => Task.FromResult(_items);

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
    }
}
