using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.Tools.Observables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NodaTime;
using NodaTime.Testing;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

public abstract class WebServiceTestsFixture : IAsyncLifetime
{
    public const string DefaultTestEnvironment = "IntegrationTest";

    private readonly TestWebServiceBuilder _webServiceBuilder;
    private readonly ContainersCollection _containers = new();
    private readonly SimpleAsyncEvent<ITestWebService> _onInitialization = new();
    private readonly SimpleAsyncEvent<ITestWebService> _onReset = new();
    private readonly SimpleAsyncEvent<ITestWebService> _onDisposal = new();

    public WebServiceTestsFixture(Type entryPointMarker)
    {
        _webServiceBuilder = new TestWebServiceBuilder(entryPointMarker)
            .WithEnvironment(DefaultTestEnvironment)
            .WithServices(services =>
            {
                services.RemoveAll<IClock>();
                services.AddSingleton<IClock>(Clock);
            });
    }

    public ITestWebService WebService { get; private set; }

    public FakeClock Clock { get; } = new(SystemClock.Instance.GetCurrentInstant());

    protected abstract void ConfigureFixture(WebServiceTestsFixtureBuilder builder);

    private ITestWebService StartService() => _webServiceBuilder.Build();

    public async Task InitializeAsync()
    {
        var builder = new WebServiceTestsFixtureBuilder(_webServiceBuilder, _containers, _onInitialization, _onReset, _onDisposal);
        ConfigureFixture(builder);

        await _containers.StartAll();
        WebService = StartService();
        await _onInitialization.Emit(WebService);
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        var hostedServicesToStop = WebService
            .Services
            .GetServices<IHostedService>()
            .SelectMany(h => h is IPausableHostedService p ? Some(p) : None)
            .ToList();

        foreach (var hostedService in hostedServicesToStop)
        {
            await hostedService.Pause(cancellationToken);
        }

        await _onReset.Emit(WebService);
        Clock.Reset(SystemClock.Instance.GetCurrentInstant());

        foreach (var hostedService in hostedServicesToStop)
        {
            await hostedService.Resume(cancellationToken);
        }
    }

    public async Task DisposeAsync()
    {
        await _onDisposal.Emit(WebService);
        await WebService.DisposeAsync();
        await _containers.DisposeAsync();
    }
}
