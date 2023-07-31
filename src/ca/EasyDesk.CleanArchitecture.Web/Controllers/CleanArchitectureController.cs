using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract class CleanArchitectureController : AbstractController
{
    protected T GetService<T>() where T : notnull => HttpContext.RequestServices.GetRequiredService<T>();

    protected ActionResultBuilder<T, T, Nothing> Dispatch<T>(IDispatchable<T> request) =>
        Result(() => GetService<IDispatcher>().Dispatch(request), It, _ => Nothing.Value);

    protected PaginatedActionResultBuilder<T> DispatchWithPagination<T>(IDispatchable<IPageable<T>> request, PaginationDto pagination)
    {
        var paginationService = GetService<PaginationService>();
        var pageIndex = paginationService.GetPageIndex(pagination.PageIndex.AsOption());
        var pageSize = paginationService.GetPageSize(pagination.PageSize.AsOption());
        return PaginatedResult(() => GetService<IDispatcher>().Dispatch(request).ThenMapAsync(p => p.GetPage(pageSize, pageIndex)), pageIndex, pageSize);
    }

    protected ActionResultBuilder<TResult, TDto, TMeta> Result<TResult, TDto, TMeta>(
        AsyncFunc<Result<TResult>> request,
        Func<TResult, TDto> mapper,
        Func<Result<TResult>, TMeta> meta) =>
            new(request, mapper, meta, this);

    protected PaginatedActionResultBuilder<TDto> PaginatedResult<TDto>(
        AsyncFunc<Result<IEnumerable<TDto>>> request,
        int pageIndex,
        int pageSize) =>
            new(request, result => PaginationMetaDto.FromResult(result, pageSize, pageIndex), this);

    protected Task<ActionResult<ResponseDto<TDto, Nothing>>> Success<TDto>(TDto result) =>
        Result<TDto, TDto, Nothing>(() => Task.FromResult(new Result<TDto>(result)), It, _ => Nothing.Value)
            .ReturnOk();

    protected Task<ActionResult<ResponseDto<TDto, Nothing>>> Failure<TDto>(Error error) =>
        Result<TDto, TDto, Nothing>(() => Task.FromResult(new Result<TDto>(error)), It, _ => Nothing.Value)
            .ReturnOk();

    protected Task<ActionResult<ResponseDto<TDto, Nothing>>> Failure<TDto>(Error error, params Error[] secondaryErrors) =>
        Failure<TDto>(Errors.Multiple(error, secondaryErrors));
}
