using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Containers;

public sealed class ContainersCollection : IAsyncDisposable
{
    private readonly ISet<ITestcontainersContainer> _containers = new HashSet<ITestcontainersContainer>();

    public T RegisterTestContainer<T>(T container)
        where T : ITestcontainersContainer
    {
        _containers.Add(container);
        return container;
    }

    public T RegisterTestContainer<T>(Func<ITestcontainersBuilder<T>, ITestcontainersBuilder<T>> configureContainer)
        where T : ITestcontainersContainer
    {
        var container = configureContainer(new TestcontainersBuilder<T>()).Build();
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

    private async Task ForEachContainer(AsyncAction<ITestcontainersContainer> action)
    {
        await Task.WhenAll(_containers.Select(c => action(c)));
    }
}
