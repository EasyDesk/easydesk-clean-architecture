namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationMetaDto
{
    public required int Count { get; init; }

    public required int PageIndex { get; init; }

    public required int PageSize { get; init; }
}
