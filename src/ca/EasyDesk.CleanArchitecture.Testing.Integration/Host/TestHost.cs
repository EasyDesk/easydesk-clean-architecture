using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Host;

internal sealed class TestHost<T> : ITestHost
    where T : class
{
    private readonly Lazy<TestHostApplicationFactory<T>> _factory;
    private ILifetimeScope? _lifetimeScope;

    public TestHost(Lazy<TestHostApplicationFactory<T>> factory)
    {
        _factory = factory;
    }

    public ILifetimeScope LifetimeScope => _lifetimeScope ?? throw new InvalidOperationException("Accessing host's LifetimeScope before starting host.");

    public Task Start()
    {
        _factory.Value.Start();
        _lifetimeScope = _factory.Value.Services.GetRequiredService<ILifetimeScope>();

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.Value.DisposeAsync();
    }
}
