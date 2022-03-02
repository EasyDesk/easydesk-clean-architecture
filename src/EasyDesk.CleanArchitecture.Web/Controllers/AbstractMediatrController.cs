using AutoMapper;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract partial class AbstractMediatrController : AbstractController
{
    private IMediator _mediator;
    private IMapper _mapper;

    private IMediator Mediator => _mediator ??= GetService<IMediator>();

    protected IMapper Mapper => _mapper ??= GetService<IMapper>();

    private T GetService<T>() => HttpContext.RequestServices.GetRequiredService<T>();

    protected Task<T> Send<T>(IRequest<T> request) => Mediator.Send(request);

    protected ActionResultBuilder<T> ForResult<T>(Result<T> result) =>
        new(result, Nothing.Value, this);

    protected ActionResultBuilder<IEnumerable<T>> ForPageResult<T>(Result<Page<T>> result)
    {
        var meta = PaginationResponseMetaDto.FromResult(result);
        return new(result.Map(page => page.Items), meta, this);
    }
}
