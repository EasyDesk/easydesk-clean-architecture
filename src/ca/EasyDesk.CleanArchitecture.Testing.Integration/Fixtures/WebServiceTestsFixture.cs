using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.Commons.Observables;
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
    private readonly SimpleAsyncEvent<ITestWebService> _beforeEachTest = new();
    private readonly SimpleAsyncEvent<ITestWebService> _afterEachTest = new();
    private readonly SimpleAsyncEvent<ITestWebService> _onReset = new();
    private readonly SimpleAsyncEvent<ITestWebService> _onDisposal = new();
    private readonly Instant _createdInstant = SystemClock.Instance.GetCurrentInstant();

    protected WebServiceTestsFixture(Type entryPointMarker)
    {
        Clock = new(InitialInstant);

        _webServiceBuilder = new TestWebServiceBuilder(entryPointMarker)
            .WithEnvironment(DefaultTestEnvironment)
            .WithServices(services =>
            {
                services.RemoveAll<IClock>();
                services.AddSingleton<IClock>(Clock);
            });
    }

    private ITestWebService? _webService;

    public ITestWebService WebService => _webService!;

    public FakeClock Clock { get; }

    protected virtual Instant InitialInstant => _createdInstant;

    protected abstract void ConfigureFixture(WebServiceTestsFixtureBuilder builder);

    private ITestWebService StartService() => _webServiceBuilder.Build();

    public async Task InitializeAsync()
    {
        var builder = new WebServiceTestsFixtureBuilder(
            webServiceBuilder: _webServiceBuilder,
            containers: _containers,
            onInitialization: _onInitialization,
            beforeEachTest: _beforeEachTest,
            afterEachTest: _afterEachTest,
            onReset: _onReset,
            onDisposal: _onDisposal);

        ConfigureFixture(builder);

        await _containers.StartAll();
        _webService = StartService();
        await _onInitialization.Emit(WebService);
    }

    public async Task BeforeTest()
    {
        await _beforeEachTest.Emit(WebService);
    }

    public async Task AfterTest()
    {
        await _afterEachTest.Emit(WebService);
    }

    public async Task Reset()
    {
        var hostedServicesToStop = WebService
            .Services
            .GetServices<IHostedService>()
            .SelectMany(h => h is IPausableHostedService p ? Some(p) : None)
            .ToList();

        foreach (var hostedService in hostedServicesToStop)
        {
            await hostedService.Pause(CancellationToken.None);
        }

        await _onReset.Emit(WebService);
        Clock.Reset(InitialInstant);

        foreach (var hostedService in hostedServicesToStop)
        {
            await hostedService.Resume(CancellationToken.None);
        }
    }

    public async Task DisposeAsync()
    {
        await _onDisposal.Emit(WebService);
        await WebService.DisposeAsync();
        await _containers.DisposeAsync();
    }
}
