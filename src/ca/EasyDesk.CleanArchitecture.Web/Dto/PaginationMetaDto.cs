namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationMetaDto(
    int Count,
    int PageIndex,
    int PageSize)
{
    public static PaginationMetaDto FromResult(Result<int> count, int pageSize, int pageIndex) =>
        new(Count: count.Value | 0, PageIndex: pageIndex, PageSize: pageSize);
}
