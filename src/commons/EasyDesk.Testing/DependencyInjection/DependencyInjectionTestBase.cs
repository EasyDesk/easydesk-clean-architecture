using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.Testing.DependencyInjection;

public abstract class DependencyInjectionTestBase : IDisposable
{
    private readonly IServiceScope _serviceScope;

    protected DependencyInjectionTestBase()
    {
        var container = new ServiceCollection();
        ConfigureServices(container);
        var provider = container.BuildServiceProvider();
        _serviceScope = provider.CreateScope();
    }

    protected IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

    protected abstract void ConfigureServices(IServiceCollection services);

    protected T Service<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    public virtual void Dispose()
    {
        _serviceScope.Dispose();
        GC.SuppressFinalize(this);
    }
}
