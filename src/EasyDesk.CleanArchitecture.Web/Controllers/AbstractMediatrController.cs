using AutoMapper;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
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

    protected ActionResultBuilder<T> ForResponse<T>(Response<T> response) where T : class =>
        new(response, Nothing.Value, this);

    protected ActionResultBuilder<IEnumerable<T>> ForPageResponse<T>(Response<Page<T>> response)
    {
        var meta = PaginationResponseMetaDto.FromResponse(response);
        return new(response.Map(page => page.Items), meta, this);
    }
}
