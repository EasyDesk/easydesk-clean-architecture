using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public class ServiceRegistry
{
    private Action<IServiceCollection>? _configureServices;
    private Action<ContainerBuilder>? _configureContainer;

    public ServiceRegistry ConfigureServices(Action<IServiceCollection> action)
    {
        _configureServices += action;
        return this;
    }

    public ServiceRegistry ConfigureContainer(Action<ContainerBuilder> action)
    {
        _configureContainer += action;
        return this;
    }

    public void ApplyToServiceCollection(IServiceCollection services)
    {
        _configureServices?.Invoke(services);
    }

    public void ApplyToContainerBuilder(ContainerBuilder builder)
    {
        _configureContainer?.Invoke(builder);
    }
}
