namespace EasyDesk.CleanArchitecture.Application.Pages;

public static class PageFactories
{
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
