using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using NodaTime;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration;

public abstract class AbstractIntegrationTest<T, TStartup> : IAsyncLifetime
    where T : IntegrationTestsWebApplicationFactory<TStartup>
    where TStartup : class
{
    protected AbstractIntegrationTest(T factory)
    {
        Factory = factory;
        Http = factory.CreateHttpHelper();
    }

    protected T Factory { get; }

    protected HttpTestHelper Http { get; private set; }

    protected RebusTestHelper NewBus(string inputQueueAddress = null, Duration? defaultTimeout = null) =>
        Factory.CreateRebusHelper(inputQueueAddress, defaultTimeout);

    public async Task InitializeAsync() => await OnInitialization();

    protected virtual Task OnInitialization() => Task.CompletedTask;

    public async Task DisposeAsync() => await OnDisposal();

    protected virtual Task OnDisposal() => Task.CompletedTask;
}
