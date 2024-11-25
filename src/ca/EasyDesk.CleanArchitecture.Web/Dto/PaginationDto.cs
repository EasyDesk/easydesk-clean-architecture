namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationDto
{
    public int? PageIndex { get; init; }

    public int? PageSize { get; init; }
}
