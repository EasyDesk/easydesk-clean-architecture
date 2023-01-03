using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using NodaTime.Testing;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration;

public abstract class WebServiceTestsFixture : IAsyncLifetime
{
    public const string DefaultTestEnvironment = "IntegrationTest";

    private readonly ContainersCollection _containers = new();

    public WebServiceTestsFixture()
    {
    }

    public ITestWebService WebService { get; private set; }

    public FakeClock Clock { get; } = new(SystemClock.Instance.GetCurrentInstant());

    protected abstract Type WebServiceEntryPointMarker { get; }

    protected virtual void ConfigureWebService(TestWebServiceBuilder builder)
    {
    }

    private ITestWebService StartNewServiceInstance()
    {
        var builder = new TestWebServiceBuilder(WebServiceEntryPointMarker)
            .WithEnvironment(DefaultTestEnvironment)
            .WithServices(services =>
            {
                services.RemoveAll<IClock>();
                services.AddSingleton<IClock>(Clock);
            });

        ConfigureWebService(builder);
        return builder.Build();
    }

    protected TContainer RegisterTestContainer<TContainer>(Func<ITestcontainersBuilder<TContainer>, ITestcontainersBuilder<TContainer>> configureContainer)
        where TContainer : ITestcontainersContainer
    {
        return _containers.RegisterTestContainer(configureContainer);
    }

    public async Task InitializeAsync()
    {
        await _containers.StartAll();
        WebService = StartNewServiceInstance();
        await OnInitialization();
    }

    public async Task ResetAsync()
    {
        await OnReset();
        Clock.Reset(SystemClock.Instance.GetCurrentInstant());
    }

    public async Task DisposeAsync()
    {
        await WebService.DisposeAsync();
        await _containers.DisposeAsync();
        await OnDisposal();
    }

    protected virtual Task OnInitialization() => Task.CompletedTask;

    protected virtual Task OnReset() => Task.CompletedTask;

    protected virtual Task OnDisposal() => Task.CompletedTask;
}
