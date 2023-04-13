namespace EasyDesk.CleanArchitecture.Web.Controllers;

public sealed class PaginationService
{
    public PaginationService(int defaultPageSize, int maxPageSize)
    {
        DefaultPageSize = defaultPageSize;
        MaxPageSize = maxPageSize;
    }

    public int DefaultPageSize { get; }

    public int MaxPageSize { get; }

    public int GetPageSize(Option<int> requestedPageSize)
    {
        return requestedPageSize
            .Map(r => Math.Clamp(r, 1, MaxPageSize))
            .OrElse(DefaultPageSize);
    }

    public int GetPageIndex(Option<int> requestedPageIndex)
    {
        return requestedPageIndex
            .Map(r => Math.Clamp(r, 0, int.MaxValue))
            .OrElse(0);
    }
}
