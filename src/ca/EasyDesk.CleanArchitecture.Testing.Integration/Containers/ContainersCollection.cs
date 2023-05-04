using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Containers;

public sealed class ContainersCollection : IAsyncDisposable
{
    private readonly ISet<IContainer> _containers = new HashSet<IContainer>();

    public T RegisterTestContainer<T>(T container)
        where T : IContainer
    {
        _containers.Add(container);
        return container;
    }

    public TContainer RegisterTestContainer<TBuilder, TContainer>(Func<TBuilder, TBuilder> configureContainer)
        where TContainer : IContainer
        where TBuilder : ContainerBuilder<TBuilder, TContainer, IContainerConfiguration>, new()
    {
        var container = configureContainer(new TBuilder()).Build();
        return RegisterTestContainer(container);
    }

    public async Task StartAll()
    {
        await ForEachContainer(async c => await c.StartAsync());
    }

    public async Task StopAll()
    {
        await ForEachContainer(async c => await c.StopAsync());
    }

    public async ValueTask DisposeAsync()
    {
        await ForEachContainer(async c => await c.DisposeAsync());
        GC.SuppressFinalize(this);
    }

    private async Task ForEachContainer(AsyncAction<IContainer> action)
    {
        await Task.WhenAll(_containers.Select(c => action(c)));
    }
}
