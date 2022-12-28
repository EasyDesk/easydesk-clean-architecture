using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using NodaTime;
using NodaTime.Testing;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration;

public abstract class AbstractIntegrationTest<T, TStartup> : IAsyncLifetime
    where T : IntegrationTestsWebApplicationFactory<TStartup>
    where TStartup : class
{
    protected AbstractIntegrationTest(T factory)
    {
        Factory = factory;
        Http = factory.CreateHttpHelper(ConfigureHttpRequest);
    }

    protected T Factory { get; }

    protected HttpTestHelper Http { get; private set; }

    protected FakeClock Clock => Factory.Clock;

    protected RebusTestHelper NewBus(string inputQueueAddress = null, Duration? defaultTimeout = null) =>
        Factory.CreateRebusHelper(inputQueueAddress, defaultTimeout);

    private void ConfigureHttpRequest(HttpRequestBuilder req)
    {
        ConfigureRequests(req);
    }

    protected virtual void ConfigureRequests(HttpRequestBuilder req)
    {
    }

    public async Task InitializeAsync()
    {
        Clock.Reset(SystemClock.Instance.GetCurrentInstant());
        await OnInitialization();
    }

    protected virtual Task OnInitialization() => Task.CompletedTask;

    public async Task DisposeAsync() => await OnDisposal();

    protected virtual Task OnDisposal() => Task.CompletedTask;
}
