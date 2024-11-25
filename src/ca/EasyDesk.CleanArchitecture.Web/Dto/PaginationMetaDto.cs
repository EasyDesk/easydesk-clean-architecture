using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationMetaDto
{
    public required int Count { get; init; }

    public required int PageIndex { get; init; }

    public required int PageSize { get; init; }

    public static PaginationMetaDto FromResult(Result<int> count, int pageSize, int pageIndex) =>
        new() { Count = count.Value | 0, PageIndex = pageIndex, PageSize = pageSize };
}
