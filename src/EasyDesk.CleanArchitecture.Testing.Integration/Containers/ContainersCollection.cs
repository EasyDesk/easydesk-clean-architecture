using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Containers;

public class ContainersCollection : IAsyncDisposable
{
    private readonly ISet<ITestcontainersContainer> _containers = new HashSet<ITestcontainersContainer>();

    public TContainer RegisterTestContainer<TContainer>(Func<ITestcontainersBuilder<TContainer>, ITestcontainersBuilder<TContainer>> configureContainer)
        where TContainer : ITestcontainersContainer
    {
        var container = configureContainer(new TestcontainersBuilder<TContainer>()).Build();
        _containers.Add(container);
        return container;
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
