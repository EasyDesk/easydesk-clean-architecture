using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Application.Pagination;

public static class PageableExtensions
{
    private class MappedPageable<T, R> : IPageable<R>
    {
        private readonly IPageable<T> _source;
        private readonly Func<T, R> _mapper;

        public MappedPageable(IPageable<T> source, Func<T, R> mapper)
        {
            _source = source;
            _mapper = mapper;
        }

        public Task<int> GetTotalCount() => _source.GetTotalCount();

        public async Task<IEnumerable<R>> GetPage(int pageSize, int pageIndex)
        {
            var page = await _source.GetPage(pageSize, pageIndex);
            return page.Select(_mapper);
        }

        public IAsyncEnumerable<IEnumerable<R>> GetAllPages(int pageSize) => _source.GetAllPages(pageSize).Select(p => p.Select(_mapper));
    }

    public static IPageable<R> Map<T, R>(this IPageable<T> source, Func<T, R> mapper) => new MappedPageable<T, R>(source, mapper);
}
