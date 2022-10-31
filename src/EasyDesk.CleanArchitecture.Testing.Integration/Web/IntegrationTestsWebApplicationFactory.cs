using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Rebus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public abstract class IntegrationTestsWebApplicationFactory<T> : WebApplicationFactory<T>, IAsyncLifetime
    where T : class
{
    private readonly ContainersContext _containers = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(ConfigureConfiguration);
        return base.CreateHost(builder);
    }

    protected virtual void ConfigureConfiguration(IConfigurationBuilder config)
    {
    }

    public TContainer RegisterTestContainer<TContainer>(Func<ITestcontainersBuilder<TContainer>, ITestcontainersBuilder<TContainer>> configureContainer)
        where TContainer : ITestcontainersContainer
    {
        return _containers.RegisterTestContainer(configureContainer);
    }

    public HttpTestHelper CreateHttpHelper()
    {
        var jsonSettings = Services.GetRequiredService<JsonSettingsConfigurator>();
        return new(CreateClient(), jsonSettings);
    }

    public RebusTestHelper CreateRebusHelper(string inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var options = Services.GetRequiredService<RebusMessagingOptions>();
        var endpoint = new RebusEndpoint(inputQueueAddress ?? GenerateNewRandomAddress());
        return new RebusTestHelper(
            rebus => rebus.ConfigureStandardBehavior(endpoint, options, Services),
            defaultTimeout);
    }

    private string GenerateNewRandomAddress() => $"rebus-test-helper-{Guid.NewGuid()}";

    public async Task InitializeAsync()
    {
        await _containers.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _containers.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
