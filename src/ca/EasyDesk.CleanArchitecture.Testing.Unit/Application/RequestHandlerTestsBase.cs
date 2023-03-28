using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Testing.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Testing.Unit.Application;

public abstract class RequestHandlerTestsBase<THandler, TRequest, TResponse> : DependencyInjectionTestBase
    where THandler : class, IHandler<TRequest, TResponse>
    where TRequest : IDispatchable<TResponse>
    where TResponse : notnull
{
    public RequestHandlerTestsBase()
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<THandler>();
    }

    protected THandler CreateHandler() => Service<THandler>();

    protected Task<Result<TResponse>> Send(TRequest request) => CreateHandler().Handle(request);
}
