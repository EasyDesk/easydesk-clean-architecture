namespace EasyDesk.CleanArchitecture.Web.Dto
{
    public record PaginationResponseMetaDto(
        int PageIndex,
        int PageSize,
        int RowCount,
        int PageCount);
}
