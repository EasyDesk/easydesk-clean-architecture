namespace EasyDesk.CleanArchitecture.Application.Pagination;

public record PageInfo<T>(IEnumerable<T> Page, int TotalCount, int PageSize, int PageIndex);
