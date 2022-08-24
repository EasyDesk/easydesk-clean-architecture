using EasyDesk.CleanArchitecture.Application.Pages;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationResponseMetaDto(
    int PageIndex,
    int PageSize,
    int RowCount,
    int PageCount)
{
    public static PaginationResponseMetaDto FromPage<T>(Page<T> page) =>
        new(page.Pagination.PageIndex, page.Pagination.PageSize, page.Count, page.PageCount);

    public static PaginationResponseMetaDto FromFailure(Error error) =>
        new(PageIndex: 0, PageSize: 0, RowCount: 0, PageCount: 0);

    public static PaginationResponseMetaDto FromResult<T>(Result<Page<T>> result) =>
        result.Match(success: FromPage, failure: FromFailure);
}
