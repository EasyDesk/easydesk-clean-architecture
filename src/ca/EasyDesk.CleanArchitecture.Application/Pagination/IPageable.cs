namespace EasyDesk.CleanArchitecture.Application.Pagination;

public interface IPageable<T>
{
    Task<int> GetTotalCount();

    Task<IEnumerable<T>> GetPage(int pageSize, int pageIndex);

    IAsyncEnumerable<IEnumerable<T>> GetAllPages(int pageSize);
}
