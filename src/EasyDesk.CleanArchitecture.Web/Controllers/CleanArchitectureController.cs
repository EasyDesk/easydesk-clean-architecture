using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract class CleanArchitectureController : AbstractController
{
    private T GetService<T>() => HttpContext.RequestServices.GetRequiredService<T>();

    protected ActionResultBuilder<T, T, Nothing> Dispatch<T>(IDispatchable<T> request) =>
        new(() => Handle(request), It, _ => Nothing.Value, this);

    protected ActionResultBuilder<PageInfo<T>, IEnumerable<T>, PaginationMetaDto> DispatchWithPagination<T>(
        IDispatchable<IPageable<T>> request, PaginationDto pagination)
    {
        var paginationService = GetService<PaginationService>();
        var pageSize = paginationService.GetPageSize(pagination.PageSize.AsOption());
        var pageIndex = paginationService.GetPageIndex(pagination.PageIndex.AsOption());
        return new(
            () => Handle(request).ThenMapAsync(pageable => pageable.GetPageInfo(pageSize, pageIndex)),
            pageInfo => pageInfo.Page,
            pageInfo => PaginationMetaDto.FromResult(pageInfo, pageSize, pageIndex),
            this);
    }

    private async Task<Result<T>> Handle<T>(IDispatchable<T> request) =>
        await GetService<IDispatcher>().Dispatch(request);
}
