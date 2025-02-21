using Autofac;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Dispatching;

internal class CustomScopeDispatcher : IDispatcher
{
    private readonly ILifetimeScope _scope;
    private readonly Action<ContainerBuilder> _setupScope;

    public CustomScopeDispatcher(ILifetimeScope scope, Action<ContainerBuilder> setupScope)
    {
        _scope = scope;
        _setupScope = setupScope;
    }

    public async Task<Result<R>> Dispatch<X, R>(IDispatchable<X> dispatchable, AsyncFunc<X, R> mapper)
    {
        await using (var scope = _scope.BeginLifetimeScope(_setupScope))
        {
            return await scope.Resolve<IDispatcher>().Dispatch(dispatchable, mapper);
        }
    }
}
