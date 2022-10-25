using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public abstract class IntegrationTestsWebApplicationFactory<T> : WebApplicationFactory<T>, IAsyncLifetime
    where T : class
{
    private readonly ISet<ITestcontainersContainer> _containers = new HashSet<ITestcontainersContainer>();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(ConfigureConfiguration);
        return base.CreateHost(builder);
    }

    protected virtual void ConfigureConfiguration(IConfigurationBuilder config)
    {
    }

    protected TContainer RegisterTestContainer<TContainer>(Func<ITestcontainersBuilder<TContainer>, ITestcontainersBuilder<TContainer>> configureContainer)
        where TContainer : ITestcontainersContainer
    {
        var container = configureContainer(new TestcontainersBuilder<TContainer>()).Build();
        _containers.Add(container);
        return container;
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_containers.Select(c => c.StartAsync()));
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await Task.WhenAll(_containers.Select(c => c.StopAsync()));
    }
}
