using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public abstract class AppModule
{
    public virtual void BeforeServiceConfiguration(AppDescription app)
    {
    }

    public void Configure(AppDescription app, ServiceRegistry registry)
    {
        ConfigureRegistry(app, registry);
        registry.ConfigureServices(services => ConfigureServices(app, services));
        registry.ConfigureContainer(builder => ConfigureContainer(app, builder));
    }

    protected virtual void ConfigureRegistry(AppDescription app, ServiceRegistry registry)
    {
    }

    protected virtual void ConfigureServices(AppDescription app, IServiceCollection services)
    {
    }

    protected virtual void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
    }
}
