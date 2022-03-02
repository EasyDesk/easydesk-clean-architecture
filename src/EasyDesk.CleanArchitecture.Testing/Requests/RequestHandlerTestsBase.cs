using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.Testing.DependencyInjection;
using EasyDesk.Tools.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Requests;

public abstract class RequestHandlerTestsBase<THandler, TRequest, TResponse> : DependencyInjectionTestBase
    where THandler : class, IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : RequestBase<TResponse>
{
    public RequestHandlerTestsBase()
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<THandler>();
    }

    protected THandler CreateHandler() => Service<THandler>();

    protected Task<Result<TResponse>> Send(TRequest request) => CreateHandler().Handle(request, default);
}
