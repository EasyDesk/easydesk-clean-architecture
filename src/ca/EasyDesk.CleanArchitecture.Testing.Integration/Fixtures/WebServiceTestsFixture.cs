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

public abstract class WebServiceTestsFixture<TSelf> : IAsyncLifetime
    where TSelf : WebServiceTestsFixture<TSelf>
{
    public const string DefaultTestEnvironment = "IntegrationTest";

    private readonly TestWebServiceBuilder _webServiceBuilder;
    private readonly ContainersCollection _containers = new();
    private readonly SimpleAsyncEvent<TSelf> _onInitialization = new();
    private readonly SimpleAsyncEvent<TSelf> _beforeEachTest = new();
    private readonly SimpleAsyncEvent<TSelf> _afterEachTest = new();
    private readonly SimpleAsyncEvent<TSelf> _onReset = new();
    private readonly SimpleAsyncEvent<TSelf> _onDisposal = new();
    private readonly Instant _createdInstant = SystemClock.Instance.GetCurrentInstant();
    private readonly Dictionary<Type, object> _seedingResults = new();

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

    protected abstract void ConfigureFixture(WebServiceTestsFixtureBuilder<TSelf> builder);

    private ITestWebService StartService() => _webServiceBuilder.Build();

    public async Task InitializeAsync()
    {
        var builder = new WebServiceTestsFixtureBuilder<TSelf>(
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
        await _onInitialization.Emit(GetSelf());
    }

    private TSelf GetSelf() => (TSelf)this;

    public async Task BeforeTest()
    {
        await _beforeEachTest.Emit(GetSelf());
    }

    public async Task AfterTest()
    {
        await _afterEachTest.Emit(GetSelf());
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

        await _onReset.Emit(GetSelf());
        Clock.Reset(InitialInstant);

        foreach (var hostedService in hostedServicesToStop)
        {
            await hostedService.Resume(CancellationToken.None);
        }
    }

    public async Task DisposeAsync()
    {
        await _onDisposal.Emit(GetSelf());
        await WebService.DisposeAsync();
        await _containers.DisposeAsync();
    }

    public T GetSeedingResult<T>()
    {
        return (T)_seedingResults[typeof(T)];
    }

    internal void UpdateSeedingResult<T>(T data) where T : notnull
    {
        _seedingResults[typeof(T)] = data;
    }
}
