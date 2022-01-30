using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.Testing.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Testing.Requests;

public abstract class UnitOfWorkHandlerTestsBase<THandler, TRequest, TResponse> : RequestHandlerTestsBase<THandler, TRequest, TResponse>
    where THandler : UnitOfWorkHandler<TRequest, TResponse>
    where TRequest : CommandBase<TResponse>
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSubstituteFor<IUnitOfWork>();
    }
}
