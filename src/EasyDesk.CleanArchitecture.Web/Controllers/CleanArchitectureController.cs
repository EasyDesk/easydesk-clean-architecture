using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract class CleanArchitectureController : AbstractController
{
    public const int DefaultPageSize = 100;

    private T GetService<T>() => HttpContext.RequestServices.GetRequiredService<T>();

    protected ActionResultBuilder<T, T> Dispatch<T>(IDispatchable<T> request) =>
        new(() => Handle(request), It, _ => Nothing.Value, this);

    protected ActionResultBuilder<PageInfo<T>, IEnumerable<T>> DispatchWithPagination<T>(
        IDispatchable<Pageable<T>> request, PaginationDto pagination)
    {
        var pageSize = pagination.PageSize ?? DefaultPageSize;
        var pageIndex = pagination.PageIndex ?? 0;
        return new(
            () => Handle(request).ThenMapAsync(pageable => pageable.GetPageInfo(pageSize, pageIndex)),
            pageInfo => pageInfo.Page,
            pageInfo => PaginationMetaDto.FromResult(pageInfo, pageSize, pageIndex),
            this);
    }

    private async Task<Result<T>> Handle<T>(IDispatchable<T> request) =>
        await GetService<IDispatcher>().Dispatch(request);
}
