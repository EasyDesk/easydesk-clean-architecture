namespace EasyDesk.CleanArchitecture.Application.Pagination;

public interface IPageable<T>
{
    Task<IEnumerable<T>> GetPage(int pageSize, int pageIndex);

    Task<IEnumerable<T>> GetAll();
}
