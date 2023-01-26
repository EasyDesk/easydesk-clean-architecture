using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract class CleanArchitectureController : AbstractController
{
    private T GetService<T>() where T : notnull => HttpContext.RequestServices.GetRequiredService<T>();

    protected ActionResultBuilder<T, T, Nothing> Dispatch<T>(IDispatchable<T> request) where T : notnull =>
        DispatchInternal(d => d.Dispatch(request), It, _ => Nothing.Value);

    protected ActionResultBuilder<PageInfo<T>, IEnumerable<T>, PaginationMetaDto> DispatchWithPagination<T>(
        IDispatchable<IPageable<T>> request, PaginationDto pagination)
    {
        var paginationService = GetService<PaginationService>();
        var pageSize = paginationService.GetPageSize(StaticImports.AsOption<int>(pagination.PageSize));
        var pageIndex = paginationService.GetPageIndex(pagination.PageIndex.AsOption());
        return DispatchInternal(
            d => d.Dispatch(request, pageable => pageable.GetPageInfo(pageSize, pageIndex)),
            pageInfo => pageInfo.Page,
            pageInfo => PaginationMetaDto.FromResult(pageInfo, pageSize, pageIndex));
    }

    private ActionResultBuilder<TResult, TDto, TMeta> DispatchInternal<TResult, TDto, TMeta>(
        AsyncFunc<IDispatcher, Result<TResult>> request,
        Func<TResult, TDto> mapper,
        Func<Result<TResult>, TMeta> meta)
        where TDto : notnull
        where TResult : notnull =>
        new(() => request(GetService<IDispatcher>()), mapper, meta, this);

    protected Task<ActionResult<ResponseDto<TDto, Nothing>>> Failure<TDto>(Error error) where TDto : notnull =>
        new ActionResultBuilder<TDto, TDto, Nothing>(() => Task.FromResult(new Result<TDto>(error)), It, _ => Nothing.Value, this).ReturnOk();

    protected Task<ActionResult<ResponseDto<TDto, Nothing>>> Failure<TDto>(Error error, params Error[] secondaryErrors) where TDto : notnull =>
        Failure<TDto>(Errors.Multiple(error, secondaryErrors));
}
