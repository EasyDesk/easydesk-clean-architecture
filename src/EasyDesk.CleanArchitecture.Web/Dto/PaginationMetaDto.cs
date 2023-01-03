using EasyDesk.CleanArchitecture.Application.Pagination;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationMetaDto(
    int PageIndex,
    int PageSize,
    int TotalCount,
    int PageCount)
{
    public static PaginationMetaDto FromPageInfo<T>(PageInfo<T> page, int pageSize, int pageIndex) =>
        new(pageIndex, pageSize, page.TotalCount, (page.TotalCount + pageSize - 1) / pageSize);

    public static PaginationMetaDto FromFailure(int pageSize, int pageIndex) =>
        new(PageIndex: pageIndex, PageSize: pageSize, TotalCount: 0, PageCount: 0);

    public static PaginationMetaDto FromResult<T>(Result<PageInfo<T>> result, int pageSize, int pageIndex) =>
        result.Match(
            success: t => FromPageInfo(t, t.PageSize, t.PageIndex),
            failure: _ => FromFailure(pageSize, pageIndex));
}
