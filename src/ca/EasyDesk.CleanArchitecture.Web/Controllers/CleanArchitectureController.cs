using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract class CleanArchitectureController : AbstractController
{
    protected T GetService<T>() where T : notnull => HttpContext.RequestServices.GetRequiredService<T>();

    protected ActionResultBuilder<T, Nothing> Dispatch<T>(IDispatchable<T> request) =>
        Result(() => DispatchRequest(request), _ => Nothing.Value);

    protected Task<Result<T>> DispatchRequest<T>(IDispatchable<T> request) =>
        GetService<IDispatcher>().Dispatch(request);

    protected ActionResultBuilder<IEnumerable<T>, PaginationMetaDto> DispatchWithPagination<T>(IDispatchable<IPageable<T>> request, PaginationDto pagination) =>
         PaginatedResult(page => GetService<IDispatcher>().Dispatch(request, x => x.GetPage(page.Size, page.Index)), e => e.Count(), pagination);

    protected ActionResultBuilder<TDto, PaginationMetaDto> PaginatedResult<TDto>(
        AsyncFunc<PageRequested, Result<TDto>> request,
        Func<TDto, int> count,
        PaginationDto pagination)
    {
        var paginationService = GetService<PaginationService>();
        var pageSize = paginationService.GetPageSize(pagination.PageSize.AsOption());
        var pageIndex = paginationService.GetPageIndex(pagination.PageIndex.AsOption());
        return Result(() => request(new(pageSize, pageIndex)), result => PaginationMetaDto.FromResult(result.Map(count), pageSize, pageIndex));
    }

    protected ActionResultBuilder<TDto, TMeta> Result<TDto, TMeta>(
        AsyncFunc<Result<TDto>> request,
        Func<Result<TDto>, TMeta> meta) =>
            new(request, meta, this);

    protected Task<ActionResult<ResponseDto<TDto, Nothing>>> Success<TDto>(TDto result) =>
        Result(() => Task.FromResult(new Result<TDto>(result)), _ => Nothing.Value)
            .ReturnOk();

    protected Task<ActionResult<ResponseDto<TDto, Nothing>>> Failure<TDto>(Error error) =>
        Result(() => Task.FromResult(new Result<TDto>(error)), _ => Nothing.Value)
            .ReturnOk();

    protected Task<ActionResult<ResponseDto<TDto, Nothing>>> Failure<TDto>(Error error, params Error[] secondaryErrors) =>
        Failure<TDto>(Errors.Multiple(error, secondaryErrors));
}
