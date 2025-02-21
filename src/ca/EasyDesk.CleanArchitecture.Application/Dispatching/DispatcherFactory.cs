using Autofac;

namespace EasyDesk.CleanArchitecture.Application.Dispatching;

public class DispatcherFactory
{
    private readonly ILifetimeScope _scope;

    public DispatcherFactory(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public IDispatcher CreateDispatcherWithCustomServices(Action<ContainerBuilder> setupScope) =>
        new CustomScopeDispatcher(_scope, setupScope);
}
