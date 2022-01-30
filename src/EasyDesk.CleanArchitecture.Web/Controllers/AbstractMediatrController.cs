using AutoMapper;
using EasyDesk.CleanArchitecture.Application.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract partial class AbstractMediatrController : AbstractController
{
    private IMediator _mediator;
    private IMapper _mapper;

    protected IMediator Mediator => _mediator ??= GetService<IMediator>();

    protected IMapper Mapper => _mapper ??= GetService<IMapper>();

    private T GetService<T>() => HttpContext.RequestServices.GetRequiredService<T>();

    protected ActionResultBuilder<TResponse> Command<TResponse>(CommandBase<TResponse> command) =>
        MakeRequest(command);

    protected ActionResultBuilder<TResponse> Query<TResponse>(QueryBase<TResponse> query) =>
        MakeRequest(query);

    private ActionResultBuilder<TResponse> MakeRequest<TResponse>(RequestBase<TResponse> request)
    {
        return new(() => Mediator.Send(request), this);
    }
}
