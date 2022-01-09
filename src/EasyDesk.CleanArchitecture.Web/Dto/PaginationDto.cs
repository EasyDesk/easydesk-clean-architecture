using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Application.Pages;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record PaginationDto(int PageIndex = 0, int PageSize = Pagination.MaxPageSize)
{
    public class MappingToPagination : DirectMapping<PaginationDto, Pagination>
    {
        public MappingToPagination() : base(src => new(src.PageIndex, src.PageSize))
        {
        }
    }
}
