namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationMetaDto(
    int Count,
    int PageIndex,
    int PageSize)
{
    public static PaginationMetaDto FromResult<T>(Result<IEnumerable<T>> result, int pageSize, int pageIndex) =>
        result.Match<PaginationMetaDto>(
            success: t => new(Count: t.Count(), PageIndex: pageIndex, PageSize: pageSize),
            failure: _ => new(Count: 0, PageIndex: pageIndex, PageSize: pageSize));
}
