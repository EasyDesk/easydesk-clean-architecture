namespace EasyDesk.CleanArchitecture.Web.Controllers;

public delegate Task<Result<TDto>> PaginatedRequest<TDto>(int pageSize, int pageIndex);
