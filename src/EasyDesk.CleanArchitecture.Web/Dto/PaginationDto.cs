using EasyDesk.CleanArchitecture.Application.Pages;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationDto(int PageIndex = 0, int PageSize = Pagination.MaxPageSize)
{
    public Pagination ToPagination() => new(PageIndex, PageSize);

    public static implicit operator Pagination(PaginationDto paginationDto) => paginationDto.ToPagination();
}
