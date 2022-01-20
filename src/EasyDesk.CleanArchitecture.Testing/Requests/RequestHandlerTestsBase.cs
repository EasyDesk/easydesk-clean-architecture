using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Testing.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Requests;

public abstract class RequestHandlerTestsBase<THandler, TRequest, TResponse> : DependencyInjectionTestBase
    where THandler : RequestHandlerBase<TRequest, TResponse>
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

    protected Task<Response<TResponse>> Send(TRequest request) => CreateHandler().Handle(request, default);
}
