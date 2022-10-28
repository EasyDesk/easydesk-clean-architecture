using AutoMapper;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract class CleanArchitectureController : AbstractController
{
    public const int DefaultPageSize = 100;

    private IMapper _mapper;

    protected IMapper Mapper => _mapper ??= GetService<IMapper>();

    private T GetService<T>() => HttpContext.RequestServices.GetRequiredService<T>();

    protected ActionResultBuilder<T, T> Dispatch<T>(ICqrsRequest<T> request) =>
        new(() => Handle(request), It, _ => Nothing.Value, this);

    protected ActionResultBuilder<PageInfo<T>, IEnumerable<T>> DispatchWithPagination<T>(
        ICqrsRequest<Pageable<T>> request, PaginationDto pagination)
    {
        var pageSize = pagination.PageSize ?? DefaultPageSize;
        var pageIndex = pagination.PageIndex ?? 0;
        return new(
            () => Handle(request).ThenMapAsync(pageable => pageable.GetPageInfo(pageSize, pageIndex)),
            pageInfo => pageInfo.Page,
            pageInfo => PaginationMetaDto.FromResult(pageInfo, pageSize, pageIndex),
            this);
    }

    private async Task<Result<T>> Handle<T>(ICqrsRequest<T> request) =>
        await GetService<ICqrsRequestDispatcher>().Dispatch(request);
}
