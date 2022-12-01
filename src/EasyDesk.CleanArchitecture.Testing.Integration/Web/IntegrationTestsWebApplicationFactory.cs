using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Rebus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using Rebus.Routing.TypeBased;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public abstract class IntegrationTestsWebApplicationFactory<T> : WebApplicationFactory<T>, IAsyncLifetime
    where T : class
{
    public const string DefaultTestEnvironment = "IntegrationTest";

    private readonly ContainersContext _containers = new();

    public IntegrationTestsWebApplicationFactory()
    {
    }

    public HttpClient HttpClient { get; private set; }

    protected virtual string Environment => DefaultTestEnvironment;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(ConfigureConfiguration);
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environment);
    }

    protected virtual void ConfigureConfiguration(IConfigurationBuilder config)
    {
    }

    public TContainer RegisterTestContainer<TContainer>(Func<ITestcontainersBuilder<TContainer>, ITestcontainersBuilder<TContainer>> configureContainer)
        where TContainer : ITestcontainersContainer
    {
        return _containers.RegisterTestContainer(configureContainer);
    }

    public HttpTestHelper CreateHttpHelper(Action<HttpRequestBuilder> configure = null)
    {
        var jsonSettings = Services.GetRequiredService<JsonSettingsConfigurator>();
        return new(HttpClient, jsonSettings, configure);
    }

    public RebusTestHelper CreateRebusHelper(string inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var options = Services.GetRequiredService<RebusMessagingOptions>();
        var appEndpoint = Services.GetRequiredService<RebusEndpoint>();
        var endpoint = new RebusEndpoint(inputQueueAddress ?? GenerateNewRandomAddress());
        return new RebusTestHelper(
            rebus => rebus
                .ConfigureStandardBehavior(endpoint, options, Services)
                .Routing(r => r.TypeBased().MapFallback(appEndpoint.InputQueueAddress)),
            defaultTimeout);
    }

    private string GenerateNewRandomAddress() => $"rebus-test-helper-{Guid.NewGuid()}";

    public virtual async Task InitializeAsync()
    {
        await _containers.StartAsync();
        HttpClient = CreateClient();
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _containers.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
